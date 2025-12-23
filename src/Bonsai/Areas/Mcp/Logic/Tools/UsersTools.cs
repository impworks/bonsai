using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Users;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Data.Models;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for managing users.
/// </summary>
[McpServerToolType]
public class UsersTools(
    UsersManagerService usersManagerService,
    McpToolAuthorizationService authService,
    McpUserContext userContext)
{
    /// <summary>
    /// Lists all users. Requires Admin role.
    /// </summary>
    [McpServerTool(Name = "list_users")]
    [Description("List all registered users. Requires Admin role.")]
    public async Task<ListUsersResult> ListUsers(
        [Description("Filter by roles (comma-separated: Unvalidated, User, Editor, Admin)")] string roles = null,
        [Description("Search query to filter by name or email")] string searchQuery = null,
        [Description("Field to order by (FullName, Email, Role, LockoutEnd)")] string orderBy = "FullName",
        [Description("Order descending (default: false)")] bool orderDescending = false,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.Admin);

        var rolesList = string.IsNullOrEmpty(roles)
            ? null
            : roles.Split(',')
                   .Select(r => Enum.TryParse<UserRole>(r.Trim(), true, out var ur) ? ur : (UserRole?)null)
                   .Where(r => r.HasValue)
                   .Select(r => r.Value)
                   .ToArray();

        var request = new UsersListRequestVM
        {
            Roles = rolesList,
            SearchQuery = searchQuery,
            OrderBy = orderBy,
            OrderDescending = orderDescending,
            Page = page
        };

        var result = await usersManagerService.GetUsersAsync(request);

        return new ListUsersResult
        {
            Users = result.Items.Select(u => new UserListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                PageId = u.PageId
            }).ToList(),
            TotalPages = result.PageCount,
            CurrentPage = request.Page
        };
    }

    /// <summary>
    /// Reads user details. Can read own profile or any user if Admin.
    /// </summary>
    [McpServerTool(Name = "read_user")]
    [Description("Read user details. Regular users can only read their own profile, Admins can read any user.")]
    public async Task<ReadUserResult> ReadUser(
        [Description("User ID (leave empty to read own profile)")] string userId = null)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var currentUserId = userContext.UserId;
        var targetUserId = string.IsNullOrEmpty(userId) ? currentUserId : userId;

        // Only admins can read other users
        if (targetUserId != currentUserId)
        {
            await authService.RequireRoleAsync(UserRole.Admin);
        }

        var user = await usersManagerService.GetAsync(targetUserId);

        return new ReadUserResult
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            PageId = user.PageId
        };
    }

    /// <summary>
    /// Gets the current user's role and permissions.
    /// </summary>
    [McpServerTool(Name = "get_current_user")]
    [Description("Get information about the currently authenticated user and their permissions.")]
    public async Task<CurrentUserResult> GetCurrentUser()
    {
        await authService.RequireRoleAsync(UserRole.User);

        var currentUserId = userContext.UserId;
        var role = await authService.GetCurrentRoleAsync();
        var user = await usersManagerService.GetAsync(currentUserId);

        return new CurrentUserResult
        {
            Id = currentUserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = role.ToString(),
            Permissions = new UserPermissions
            {
                CanRead = role >= UserRole.User,
                CanEdit = role >= UserRole.Editor,
                CanAdmin = role >= UserRole.Admin
            },
            PageId = user.PageId
        };
    }
}

#region Result Types

public class ListUsersResult
{
    public List<UserListItem> Users { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class UserListItem
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsLocked { get; set; }
    public Guid? PageId { get; set; }
}

public class ReadUserResult
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public Guid? PageId { get; set; }
}

public class CurrentUserResult
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public UserPermissions Permissions { get; set; }
    public Guid? PageId { get; set; }
}

public class UserPermissions
{
    public bool CanRead { get; set; }
    public bool CanEdit { get; set; }
    public bool CanAdmin { get; set; }
}

#endregion
