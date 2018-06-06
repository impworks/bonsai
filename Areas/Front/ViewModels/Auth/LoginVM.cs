namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information about the login state.
    /// </summary>
    public class LoginVM
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
        /// Flag indicating that unauthorized visitors can view page contents.
        /// </summary>
        public bool AllowGuests { get; set; }
    }
}
