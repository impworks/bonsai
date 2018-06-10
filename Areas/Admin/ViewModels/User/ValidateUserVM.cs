using Bonsai.Areas.Front.ViewModels.Auth;

namespace Bonsai.Areas.Admin.ViewModels.User
{
    /// <summary>
    /// VM for validating a user.
    /// </summary>
    public class ValidateUserVM: RegisterUserVM
    {
        /// <summary>
        /// Surrogate user ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Assigned role.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Flag indicating that the user must be granted a page.
        /// </summary>
        public bool CreatePersonalPage { get; set; }
    }
}
