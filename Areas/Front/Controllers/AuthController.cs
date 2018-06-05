using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Services;
using Bonsai.Code.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        public AuthController(AuthService auth, AppConfigService cfgProvider)
        {
            _auth = auth;
            _cfgProvider = cfgProvider;
        }

        private readonly AuthService _auth;
        private readonly AppConfigService _cfgProvider;

        protected ISession Session => HttpContext.Session;

        /// <summary>
        /// Displays the authorization page.
        /// </summary>
        [HttpGet]
        [Route("login")]
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            var user = await _auth.GetCurrentUserAsync(User);
            var vm = new LoginVM
            {
                ReturnUrl = returnUrl,
                AllowGuests = _cfgProvider.GetConfig().AllowGuests,
                Status = user?.IsValidated == false ? LoginStatus.Unvalidated : (LoginStatus?) null
            };
            return View(vm);
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
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Invoked by external login provider when the authorization is successful.
        /// </summary>
        [Route("loginCallback")]
        public async Task<ActionResult> LoginCallback(string returnUrl)
        {
            var result = await _auth.LoginAsync().ConfigureAwait(false);

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

            var vm = new LoginVM
            {
                ReturnUrl = returnUrl,
                AllowGuests = _cfgProvider.GetConfig().AllowGuests,
                Status = result.Status
            };

            HttpContext.User = result.Principal;

            return View("Login", vm);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpGet]
        [Route("register")]
        public ActionResult Register()
        {
            var vm = Session.Get<RegistrationInfo>()?.FormData ?? new RegisterUserVM();
            return View("RegisterForm", vm);
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterUserVM vm)
        {
            var info = Session.Get<RegistrationInfo>();
            if (info == null)
                return RedirectToAction("Login");

            if(!ModelState.IsValid)
                return View("RegisterForm", vm);

            var result = await _auth.RegisterAsync(vm, info.Login);
            if (result.ErrorMessages.Any())
            {
                foreach(var error in result.ErrorMessages)
                    ModelState.AddModelError(error.Key, error.Value);

                return View("RegisterForm", vm);
            }

            return RedirectToAction("RegisterSuccess", "Auth");
        }

        /// <summary>
        /// Displays the "Registration success" page.
        /// </summary>
        [Route("registerSuccess")]
        public ActionResult RegisterSuccess()
        {
            if (Session.Get<RegistrationInfo>() == null)
                return RedirectToAction("Index", "Home");

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

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
