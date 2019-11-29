using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper functions for working with users.
    /// </summary>
    public static class UserManagerExtensions
    {
        /// <summary>
        /// Returns the user or throws a "not found" error.
        /// </summary>
        public static async Task<AppUser> GetUserAsync(this UserManager<AppUser> userMgr, ClaimsPrincipal principal, string errorMessage)
        {
            return await userMgr.GetUserAsync(principal)
                   ?? throw new OperationException(errorMessage);
        }
    }
}
