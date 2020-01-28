namespace Bonsai.Areas.Admin.ViewModels.Users
{
    /// <summary>
    /// Common interface for password-related VMs.
    /// </summary>
    interface IPasswordForm
    {
        /// <summary>
        /// Password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Password copy for typo checking.
        /// </summary>
        string PasswordCopy { get; }
    }
}
