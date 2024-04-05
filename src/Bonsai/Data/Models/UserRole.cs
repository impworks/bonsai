namespace Bonsai.Data.Models
{
    /// <summary>
    /// Known account roles.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Newly registered user.
        /// </summary>
        Unvalidated,

        /// <summary>
        /// Basic user with read-only rights.
        /// </summary>
        User,

        /// <summary>
        /// User who can edit content, add pages or media.
        /// </summary>
        Editor,

        /// <summary>
        /// Almighty administrator.
        /// </summary>
        Admin
    }
}
