using System.Security.Claims;
using Bonsai.Areas.Front.Logic.Auth;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Result of an authentication attempt.
    /// </summary>
    public class LoginResultVM
    {
        public LoginResultVM(LoginStatus status, ExternalLoginInfo extLogin = null)
        {
            Status = status;

            if (extLogin != null)
            {
                Principal = extLogin.Principal;
                ExternalLogin = new ExternalLoginData(extLogin.LoginProvider, extLogin.ProviderKey);
            }
        }

        /// <summary>
        /// Status of the operation.
        /// </summary>
        public LoginStatus Status { get; }

        /// <summary>
        /// Newly authenticated user (if the log-in was successful).
        /// </summary>
        public ClaimsPrincipal Principal { get; }

        /// <summary>
        /// Information about the external login.
        /// </summary>
        public ExternalLoginData ExternalLogin { get; }
    }
}
