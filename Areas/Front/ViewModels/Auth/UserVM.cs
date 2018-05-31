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
        /// URL to the avatar.
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Flag indicating that this user has access to admin panel.
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// Reference to the user's own page.
        /// </summary>
        public string PageKey { get; set; }
    }
}
