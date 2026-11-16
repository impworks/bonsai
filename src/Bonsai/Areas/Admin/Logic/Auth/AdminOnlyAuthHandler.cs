using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Admin.Logic.Auth;

/// <summary>
/// Authorization handler that only allows users with the Administrator role.
/// </summary>
public class AdminOnlyAuthHandler: AuthorizationHandler<AdminOnlyAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyAuthRequirement requirement)
    {
        if (context.User?.IsInRole(nameof(UserRole.Admin)) == true)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}