namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Additional options for the user registration form.
    /// </summary>
    public class RegisterUserDataVM
    {
        /// <summary>
        /// Flag indicating that this created user is the first one (and will be made an admin).
        /// </summary>
        public bool IsFirstUser { get; set; }
    }
}
