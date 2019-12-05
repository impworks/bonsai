namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// Information about a user removal request.
    /// </summary>
    public class RemoveUserVM
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's full name (for clarification).
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Flag indicating that user attempts to remove their own account.
        /// </summary>
        public bool IsSelf { get; set; }

        /// <summary>
        /// Flag indicating that the user can be removed completely.
        /// Otherwise, the account is only locked.
        /// </summary>
        public bool IsFullyDeletable { get; set; }
    }
}
