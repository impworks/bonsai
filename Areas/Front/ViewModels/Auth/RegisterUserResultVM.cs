using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Result of the user's creation request.
    /// </summary>
    public class RegisterUserResultVM
    {
        /// <summary>
        /// Validation errors.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> ErrorMessages { get; set; }
    }
}
