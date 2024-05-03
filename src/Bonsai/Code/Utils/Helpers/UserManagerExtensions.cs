using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Code.Utils.Helpers;

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

    /// <summary>
    /// Checks if the user belongs to a specific role.
    /// Throws a <see cref="OperationException" /> if the user does not exist.
    /// </summary>
    public static async Task<bool> IsInRoleAsync(this UserManager<AppUser> userMgr, ClaimsPrincipal principal, UserRole role)
    {
        var user = await userMgr.GetUserAsync(principal)
                   ?? throw new OperationException(Texts.Global_Error_UserNotFound);

        return await userMgr.IsInRoleAsync(user, role.ToString());
    }
}