using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Admin.Logic.Auth
{
    /// <summary>
    /// Authorization handler for requiring login depending on the config.
    /// </summary>
    public class AdminAuthHandler: AuthorizationHandler<AdminAuthRequirement>
    {
        /// <summary>
        /// Checks the authorization if the config requires it.
        /// </summary>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminAuthRequirement requirement)
        {
            var user = context.User;
            if (user == null)
                return;

            if(user.IsInRole(nameof(UserRole.Admin)) || user.IsInRole(nameof(UserRole.Editor)))
                context.Succeed(requirement);
        }
    }
}
