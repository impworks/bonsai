namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information about the currently authorized user.
    /// </summary>
    public class UserVM
    {
        /// <summary>
        /// Readable name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's registered email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Flag indicating that this user has access to admin panel.
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// Flag indicating that the current user has been validated by administators.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Reference to the user's own page.
        /// </summary>
        public string PageKey { get; set; }
    }
}
