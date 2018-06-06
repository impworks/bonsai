using Bonsai.Areas.Front.Logic.Auth;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information about the user authorized via external provider.
    /// </summary>
    public class RegistrationInfo
    {
        /// <summary>
        /// Default values for the registration form.
        /// </summary>
        public RegisterUserVM FormData { get; set; }

        /// <summary>
        /// External login credentials.
        /// </summary>
        public ExternalLoginData Login { get; set; }
    }
}
