using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// Controller for registration/authorization pages.
    /// </summary>
    [Area("Front")]
    [Route("auth")]
    public class AuthController: Controller
    {
        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        private readonly AuthService _auth;

        private const string ExternalLoginInfoKey = "ExternalLoginInfo";
        private const string RegisterUserVMKey = "RegisterUserVM";

        private ISession Session => HttpContext.Session;

        /// <summary>
        /// Displays the authorization page.
        /// </summary>
        [HttpGet]
        [Route("login")]
        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
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
            await _auth.LogoutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Invoked by external login provider when the authorization is successful.
        /// </summary>
        [Route("loginCallback")]
        public async Task<ActionResult> LoginCallback(string returnUrl)
        {
            var authResult = await HttpContext.AuthenticateAsync(AuthService.ExternalCookieAuthType).ConfigureAwait(false);
            var info = await _auth.LoginAsync(authResult).ConfigureAwait(false);

            if (info.Status == LoginStatus.Succeeded)
                return RedirectLocal(returnUrl);

            if (info.Status == LoginStatus.NewUser)
            {
                Session.Set(ExternalLoginInfoKey, info.ExternalLogin);
                Session.Set(RegisterUserVMKey, _auth.GetRegistrationData(authResult.Principal));

                return RedirectToAction("Register");
            }

            return View("LoginResult", info.Status);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpGet]
        [Route("register")]
        public ActionResult Register()
        {
            var vm = Session.Get<RegisterUserVM>(RegisterUserVMKey) ?? new RegisterUserVM();
            return View("RegisterForm", vm);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterUserVM vm)
        {
            var extLogin = Session.Get<ExternalLoginData>(ExternalLoginInfoKey);
            if (extLogin == null)
                return RedirectToAction("Login");

            if(!ModelState.IsValid)
                return View("RegisterForm", vm);

            var result = await _auth.RegisterAsync(vm, extLogin);

            if (result.ErrorMessages.Any())
            {
                foreach(var error in result.ErrorMessages)
                    ModelState.AddModelError(error.Key, error.Value);

                return View("RegisterForm", vm);
            }

            Session.Remove(ExternalLoginInfoKey);
            Session.Remove(RegisterUserVMKey);

            return View("RegisterSuccess");
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

        #endregion
    }
}
