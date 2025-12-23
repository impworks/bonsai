using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace Bonsai.Areas.Mcp.Logic.Auth;

/// <summary>
/// Provides user context for MCP tool operations.
/// Supports both OAuth (OpenIddict) and API key authentication.
/// </summary>
public class McpUserContext(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
{
    private UserRole? _cachedRole;

    /// <summary>
    /// The current user's claims principal.
    /// </summary>
    public ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// The current user's ID.
    /// </summary>
    public string UserId => Principal?.FindFirst(OpenIddictConstants.Claims.Subject)?.Value
        ?? userManager.GetUserId(Principal);

    /// <summary>
    /// Gets the current user's role.
    /// </summary>
    public async Task<UserRole> GetRoleAsync()
    {
        if (_cachedRole.HasValue)
            return _cachedRole.Value;

        // First try to get role from the bonsai_role claim (set by OAuth)
        var bonsaiRoleClaim = Principal?.FindFirst("bonsai_role")?.Value;
        if (!string.IsNullOrEmpty(bonsaiRoleClaim) && Enum.TryParse<UserRole>(bonsaiRoleClaim, out var parsedRole))
        {
            _cachedRole = parsedRole;
            return _cachedRole.Value;
        }

        // Fall back to looking up the user
        var user = await userManager.GetUserAsync(Principal);
        if (user == null)
        {
            // Try to find user by subject claim (for OAuth tokens)
            var userId = UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                user = await userManager.FindByIdAsync(userId);
            }
        }

        if (user == null)
        {
            _cachedRole = UserRole.Unvalidated;
            return _cachedRole.Value;
        }

        // Get role from Identity
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Contains(nameof(UserRole.Admin)))
            _cachedRole = UserRole.Admin;
        else if (roles.Contains(nameof(UserRole.Editor)))
            _cachedRole = UserRole.Editor;
        else if (roles.Contains(nameof(UserRole.User)))
            _cachedRole = UserRole.User;
        else
            _cachedRole = UserRole.Unvalidated;

        return _cachedRole.Value;
    }

    /// <summary>
    /// Checks if the user has at least the specified role.
    /// </summary>
    public async Task<bool> HasRoleAsync(UserRole minimumRole)
    {
        var role = await GetRoleAsync();
        return role >= minimumRole;
    }
}
