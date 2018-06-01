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
    }
}
