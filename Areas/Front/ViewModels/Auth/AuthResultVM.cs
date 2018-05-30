using Bonsai.Areas.Front.Logic.Auth;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Result of an authentication attempt.
    /// </summary>
    public class AuthResultVM
    {
        /// <summary>
        /// Status of the operation.
        /// </summary>
        public AuthStatus Status { get; set; }

        /// <summary>
        /// Information about the external login.
        /// </summary>
        public ExternalLoginData ExternalLogin { get; set; }
    }
}
