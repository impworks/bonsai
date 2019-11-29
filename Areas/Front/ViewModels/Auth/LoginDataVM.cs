using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information about the login state.
    /// </summary>
    public class LoginDataVM
    {
        /// <summary>
        /// URL to return to after a successful authorization.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Current status.
        /// </summary>
        public LoginStatus? Status { get; set; }

        /// <summary>
        /// Flag indicating that there are no users in the database and current user will register as first.
        /// </summary>
        public bool IsFirstUser { get; set; }

        /// <summary>
        /// Flag indicating that unauthorized visitors can view page contents.
        /// </summary>
        public bool AllowGuests { get; set; }

        /// <summary>
        /// Flag indicating that login-password auth is enabled.
        /// </summary>
        public bool AllowPasswordAuth { get; set; }

        /// <summary>
        /// List of enabled authentication providers.
        /// </summary>
        public IEnumerable<AuthProviderVM> Providers { get; set; }
    }
}
