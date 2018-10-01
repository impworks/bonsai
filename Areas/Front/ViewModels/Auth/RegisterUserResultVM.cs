using System.Security.Claims;
using Bonsai.Data.Models;

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

        /// <summary>
        /// The created user profile.
        /// </summary>
        public AppUser User { get; set; }

        /// <summary>
        /// The principal for newly created user.
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }
    }
}
