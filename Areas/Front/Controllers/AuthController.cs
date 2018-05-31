using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Utils;
using Microsoft.AspNetCore.Authentication;
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

        private const string ExternalCookieAuthType = "ExternalCookie";
        private const string ExternalLoginInfoKey = "ExternalLoginInfo";

        /// <summary>
        /// Displays the authorization page.
        /// </summary>
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Sends the authorization request.
        /// </summary>
        [HttpPost]
        public ActionResult Login(string provider, string returnUrl)
        {
            var authProps = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                RedirectUri = returnUrl,
                Items = { ["LoginProvider"] = provider }
            };
            return Challenge(authProps, provider);
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _auth.LogoutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Invoked by external login provider when the authorization is successful.
        /// </summary>
        public async Task<ActionResult> LoginCallback(string returnUrl)
        {
            var authResult = await HttpContext.AuthenticateAsync(ExternalCookieAuthType).ConfigureAwait(false);
            var info = await _auth.LoginAsync(authResult).ConfigureAwait(false);

            if (info.Status == LoginStatus.Succeeded)
                return RedirectLocal(returnUrl);

            if (info.Status == LoginStatus.NewUser)
            {
                HttpContext.Session.Set(ExternalLoginInfoKey, info.ExternalLogin);
                return RedirectToAction("Register");
            }

            return View("LoginResult", info.Status);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Register()
        {
            return View("RegisterForm");
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterUserVM vm)
        {
            var extLogin = HttpContext.Session.Get<ExternalLoginData>(ExternalLoginInfoKey);
            if (extLogin == null)
                return RedirectToAction("Login");

            if(!ModelState.IsValid)
                return View("RegisterForm");

            var result = await _auth.RegisterAsync(vm, extLogin);

            if (result.ErrorMessages.Any())
            {
                foreach(var error in result.ErrorMessages)
                    ModelState.AddModelError(error.Key, error.Value);

                return View("RegisterForm");
            }

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
