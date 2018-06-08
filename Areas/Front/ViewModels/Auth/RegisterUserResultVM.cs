using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Result of the user's creation request.
    /// </summary>
    public class RegisterUserResultVM
    {
        public RegisterUserResultVM(IReadOnlyList<KeyValuePair<string, string>> msgs = null)
        {
            ErrorMessages = msgs ?? new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Validation errors.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> ErrorMessages { get; }

        /// <summary>
        /// Flag indicating the user's validation state.
        /// The first user is automatically validated.
        /// </summary>
        public bool IsValidated { get; set; }
    }
}
