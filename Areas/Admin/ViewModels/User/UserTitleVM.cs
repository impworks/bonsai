namespace Bonsai.Areas.Admin.ViewModels.User
{
    /// <summary>
    /// Brief information about the user.
    /// </summary>
    public class UserTitleVM
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Readable name of the user.
        /// </summary>
        public string FullName { get; set; }
    }
}
