namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Credentials for local authorization.
    /// </summary>
    public class LocalLoginVM
    {
        /// <summary>
        /// Account login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// URL to redirect to after logging in.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
