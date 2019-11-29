namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// VM for updating a user's password.
    /// </summary>
    public class UserPasswordEditorVM: IPasswordForm
    {
        /// <summary>
        /// ID of the user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Password copy for typo checking.
        /// </summary>
        public string PasswordCopy { get; set; }
    }
}
