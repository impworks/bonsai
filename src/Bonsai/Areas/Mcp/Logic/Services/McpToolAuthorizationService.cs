using System;
using System.Threading.Tasks;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Code.Services.Config;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Mcp.Logic.Services;

/// <summary>
/// Service for authorizing MCP tool operations based on user role.
/// </summary>
public class McpToolAuthorizationService(McpUserContext userContext, BonsaiConfigService configService)
{
    /// <summary>
    /// Ensures the current user has at least the specified role.
    /// Throws UnauthorizedAccessException if the user doesn't have sufficient permissions.
    /// </summary>
    public async Task RequireRoleAsync(UserRole minimumRole)
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            throw new InvalidOperationException("MCP server is disabled. Enable it in the admin configuration.");
        }

        var hasRole = await userContext.HasRoleAsync(minimumRole);
        if (!hasRole)
        {
            var currentRole = await userContext.GetRoleAsync();
            throw new UnauthorizedAccessException(
                $"This operation requires {minimumRole} role. Current role: {currentRole}");
        }
    }

    /// <summary>
    /// Checks if the current user has at least the specified role.
    /// </summary>
    public async Task<bool> HasRoleAsync(UserRole minimumRole)
    {
        return await userContext.HasRoleAsync(minimumRole);
    }

    /// <summary>
    /// Gets the current user's role.
    /// </summary>
    public async Task<UserRole> GetCurrentRoleAsync()
    {
        return await userContext.GetRoleAsync();
    }

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    public string GetCurrentUserId()
    {
        return userContext.UserId;
    }
}
