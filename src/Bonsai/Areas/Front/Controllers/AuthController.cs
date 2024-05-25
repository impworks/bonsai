using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Services.Search;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Controllers;

/// <summary>
/// Controller for registration/authorization pages.
/// </summary>
[Area("Front")]
[Route("auth")]
public class AuthController(
    AuthService authSvc,
    AuthProviderService authProvSvc,
    PagesManagerService pagesSvc,
    ISearchEngine search,
    BonsaiConfigService configSvc,
    IBackgroundJobService jobs,
    AppDbContext db)
    : AppControllerBase
{
    /// <summary>
    /// Displays the authorization page.
    /// </summary>
    [HttpGet]
    [Route("login")]
    public async Task<ActionResult> Login(string returnUrl = null)
    {
        if (await authSvc.IsFirstUserAsync())
            return RedirectToAction("Register");
            
        var user = await authSvc.GetCurrentUserAsync(User);
        var status = user?.IsValidated switch
        {
            true => LoginStatus.Succeeded,
            false => LoginStatus.Unvalidated,
            null => (LoginStatus?) null
        };
        if (status == LoginStatus.Succeeded)
            return RedirectToAction("Index", "Home");

        return await ViewLoginFormAsync(status, returnUrl);
    }

    /// <summary>
    /// Sends the authorization request.
    /// </summary>
    [HttpPost]
    [Route("externalLogin")]
    public ActionResult ExternalLogin(string provider, string returnUrl)
    {
        var redirectUrl = Url.Action("LoginCallback", new {returnUrl = returnUrl});
        var authProps = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = true,
            RedirectUri = redirectUrl,
            Items = { ["LoginProvider"] = provider }
        };
        return Challenge(authProps, provider);
    }

    /// <summary>
    /// Attempts to authorize the user via a login-password pair.
    /// </summary>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult> Login(LocalLoginVM vm)
    {
        var result = await authSvc.LocalLoginAsync(vm);

        if (result.Status == LoginStatus.Succeeded)
            return RedirectLocal(vm.ReturnUrl);

        return await ViewLoginFormAsync(result.Status);
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    [HttpGet]
    [Route("logout")]
    public async Task<ActionResult> Logout()
    {
        await authSvc.LogoutAsync();
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return RedirectToAction("Index", "Home", new { area = "Front" });
    }

    /// <summary>
    /// Invoked by external login provider when the authorization is successful.
    /// </summary>
    [Route("loginCallback")]
    public async Task<ActionResult> LoginCallback(string returnUrl)
    {
        var result = await authSvc.ExternalLoginAsync();

        if (result.Status == LoginStatus.Succeeded)
            return RedirectLocal(returnUrl);

        if (result.Status == LoginStatus.NewUser)
        {
            Session.Set(new RegistrationInfo
            {
                FormData = authSvc.GetRegistrationData(result.Principal),
                Login = result.ExternalLogin
            });
            return RedirectToAction("Register");
        }

        HttpContext.User = result.Principal;

        return await ViewLoginFormAsync(result.Status, returnUrl);
    }

    /// <summary>
    /// Displays the user registration form.
    /// </summary>
    [HttpGet]
    [Route("register")]
    public async Task<ActionResult> Register()
    {
        var user = await authSvc.GetCurrentUserAsync(User);
        if (user != null)
        {
            return user.IsValidated
                ? RedirectToAction("Index", "Home")
                : RedirectToAction("Login");
        }

        if (!await CanRegisterAsync())
            return View("RegisterDisabled");

        var extAuth = Session.Get<RegistrationInfo>();
        var vm = extAuth?.FormData ?? new RegisterUserVM();
        vm.CreatePersonalPage = true;
        return await ViewRegisterFormAsync(vm, usesPasswordAuth: extAuth == null);
    }

    /// <summary>
    /// Displays the user registration form.
    /// </summary>
    [HttpPost]
    [Route("register")]
    public async Task<ActionResult> Register(RegisterUserVM vm)
    {
        if (!await CanRegisterAsync())
            return View("RegisterDisabled");

        var info = Session.Get<RegistrationInfo>();

        if(!ModelState.IsValid)
            return await ViewRegisterFormAsync(vm, usesPasswordAuth: info == null);

        try
        {
            var result = await authSvc.RegisterAsync(vm, info?.Login);
            if (!result.IsValidated)
                return RedirectToAction("RegisterSuccess", "Auth");

            if (vm.CreatePersonalPage)
            {
                db.Entry(result.User).State = EntityState.Unchanged;
                result.User.Page = await pagesSvc.CreateDefaultUserPageAsync(vm, result.Principal);
                await db.SaveChangesAsync();

                await search.AddPageAsync(result.User.Page);
                await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());
            }

            return RedirectToAction("Index", "Home");
        }
        catch (ValidationException ex)
        {
            SetModelState(ex);

            return await ViewRegisterFormAsync(vm, usesPasswordAuth: info == null);
        }
    }

    /// <summary>
    /// Displays the "Registration success" page.
    /// </summary>
    [Route("registerSuccess")]
    public ActionResult RegisterSuccess()
    {
        if (Session.Get<RegistrationInfo>() == null)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        Session.Remove<RegistrationInfo>();

        return View();
    }

    /// <summary>
    /// Displays a message when external auth has failed.
    /// </summary>
    [HttpGet]
    [Route("failed")]
    public ActionResult Failed()
    {
        return View();
    }

    #region Private helpers

    /// <summary>
    /// Checks if the redirect is a local page.
    /// </summary>
    private ActionResult RedirectLocal(string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            var currHost = HttpContext.Request.Host.Host.ToLower();
            var canRedirect = Url.IsLocalUrl(returnUrl)
                              || new Uri(returnUrl, UriKind.Absolute).Host.Contains(currHost);

            if (canRedirect)
                return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Displays the registration form.
    /// </summary>
    private async Task<ActionResult> ViewRegisterFormAsync(RegisterUserVM vm, bool usesPasswordAuth)
    {
        ViewBag.Data = new RegisterUserDataVM
        {
            IsFirstUser = await authSvc.IsFirstUserAsync(),
            UsePasswordAuth = usesPasswordAuth
        };

        return View("RegisterForm", vm);
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    private async Task<ActionResult> ViewLoginFormAsync(LoginStatus? status, string returnUrl = null)
    {
        var dynCfg = configSvc.GetDynamicConfig();
        ViewBag.Data = new LoginDataVM
        {
            ReturnUrl = returnUrl,
            AllowGuests = dynCfg.AllowGuests,
            AllowRegistration = dynCfg.AllowRegistration,
            AllowPasswordAuth = configSvc.GetStaticConfig().Auth.AllowPasswordAuth,
            Providers = authProvSvc.AvailableProviders,
            IsFirstUser = await authSvc.IsFirstUserAsync(),
            Status = status
        };
        return View("Login", new LocalLoginVM());
    }

    /// <summary>
    /// Checks if the registration is allowed.
    /// </summary>
    private async Task<bool> CanRegisterAsync()
    {
        if (configSvc.GetDynamicConfig().AllowRegistration)
            return true;

        if (await authSvc.IsFirstUserAsync())
            return true;

        return false;
    }

    #endregion
}