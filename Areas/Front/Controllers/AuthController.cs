using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Elastic;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Controller for registration/authorization pages.
    /// </summary>
    [Area("Front")]
    [Route("auth")]
    public class AuthController: AppControllerBase
    {
        public AuthController(AuthService auth, AuthProviderService provs, PagesManagerService pages, ElasticService elastic, AppConfigService cfgProvider, AppDbContext db)
        {
            _auth = auth;
            _provs = provs;
            _pages = pages;
            _elastic = elastic;
            _cfgProvider = cfgProvider;
            _db = db;
        }

        private readonly AuthService _auth;
        private readonly AuthProviderService _provs;
        private readonly PagesManagerService _pages;
        private readonly ElasticService _elastic;
        private readonly AppConfigService _cfgProvider;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the authorization page.
        /// </summary>
        [HttpGet]
        [Route("login")]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            var user = await _auth.GetCurrentUserAsync(User);
            var status = user?.IsValidated == false ? LoginStatus.Unvalidated : (LoginStatus?) null;
            return ViewLoginForm(status, returnUrl);
        }

        /// <summary>
        /// Sends the authorization request.
        /// </summary>
        [HttpPost]
        [Route("login")]
        public ActionResult Login(string provider, string returnUrl)
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
        /// Logs the user out.
        /// </summary>
        [HttpGet]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            await _auth.LogoutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction("Index", "Home", new { area = "Front" });
        }

        /// <summary>
        /// Invoked by external login provider when the authorization is successful.
        /// </summary>
        [Route("loginCallback")]
        public async Task<ActionResult> LoginCallback(string returnUrl)
        {
            var result = await _auth.LoginAsync();

            if (result.Status == LoginStatus.Succeeded)
                return RedirectLocal(returnUrl);

            if (result.Status == LoginStatus.NewUser)
            {
                Session.Set(new RegistrationInfo
                {
                    FormData = _auth.GetRegistrationData(result.Principal),
                    Login = result.ExternalLogin
                });
                return RedirectToAction("Register");
            }

            HttpContext.User = result.Principal;

            return ViewLoginForm(result.Status, returnUrl);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpGet]
        [Route("register")]
        public async Task<ActionResult> Register()
        {
            if (!await CanRegisterAsync())
                return View("RegisterDisabled");

            var vm = Session.Get<RegistrationInfo>()?.FormData ?? new RegisterUserVM();
            vm.CreatePersonalPage = true;
            return await ViewRegisterFormAsync(vm);
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
            if (info == null)
                return RedirectToAction("Login");

            if(!ModelState.IsValid)
                return await ViewRegisterFormAsync(vm);

            try
            {
                var result = await _auth.RegisterAsync(vm, info.Login);
                if (!result.IsValidated)
                    return RedirectToAction("RegisterSuccess", "Auth");

                if (vm.CreatePersonalPage)
                {
                    _db.Entry(result.User).State = EntityState.Unchanged;
                    result.User.Page = await _pages.CreateDefaultUserPageAsync(vm, result.Principal);
                    await _db.SaveChangesAsync();

                    await _elastic.AddPageAsync(result.User.Page);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);

                return await ViewRegisterFormAsync(vm);
            }
        }

        /// <summary>
        /// Displays the "Registration success" page.
        /// </summary>
        [Route("registerSuccess")]
        public ActionResult RegisterSuccess()
        {
            if (Session.Get<RegistrationInfo>() == null)
                return RedirectToAction("Index", "Dashboard");

            Session.Remove<RegistrationInfo>();

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

            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Displays the registration form.
        /// </summary>
        private async Task<ActionResult> ViewRegisterFormAsync(RegisterUserVM vm)
        {
            ViewBag.Data = new RegisterUserDataVM
            {
                IsFirstUser = await _auth.IsFirstUserAsync()
            };

            return View("RegisterForm", vm);
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        private ActionResult ViewLoginForm(LoginStatus? status, string returnUrl = null)
        {
            var vm = new LoginVM
            {
                ReturnUrl = returnUrl,
                AllowGuests = _cfgProvider.GetConfig().AllowGuests,
                Providers = _provs.AvailableProviders,
                Status = status
            };
            return View(vm);
        }

        /// <summary>
        /// Checks if the registration is allowed.
        /// </summary>
        private async Task<bool> CanRegisterAsync()
        {
            if (_cfgProvider.GetConfig().AllowRegistration)
                return true;

            var isFirst = await _auth.IsFirstUserAsync();
            if (isFirst)
                return true;

            return false;
        }

        #endregion
    }
}
