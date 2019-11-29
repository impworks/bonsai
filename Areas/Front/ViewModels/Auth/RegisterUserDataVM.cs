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

        /// <summary>
        /// Flag indicating that local auth has been chosen and the user must provide a login and password.
        /// </summary>
        public bool UsePasswordAuth { get; set; }
    }
}
