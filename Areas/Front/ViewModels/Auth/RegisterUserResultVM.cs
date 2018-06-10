namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Result of the user's creation request.
    /// </summary>
    public class RegisterUserResultVM
    {
        /// <summary>
        /// Flag indicating the user's validation state.
        /// The first user is automatically validated.
        /// </summary>
        public bool IsValidated { get; set; }
    }
}
