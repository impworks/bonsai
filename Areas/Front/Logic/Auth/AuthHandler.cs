using System.Threading.Tasks;
using Bonsai.Code.Services;
using Microsoft.AspNetCore.Authorization;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Authorization handler for requiring login depending on the config.
    /// </summary>
    public class AuthHandler: AuthorizationHandler<AuthRequirement>
    {
        public AuthHandler(AppConfigService cfgProvider)
        {
            _cfgProvider = cfgProvider;
        }

        private readonly AppConfigService _cfgProvider;

        /// <summary>
        /// Checks the authorization if the config requires it.
        /// </summary>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthRequirement requirement)
        {
            var cfg = _cfgProvider.GetConfig();
            var user = context.User.Identity;

            if(cfg.AllowGuests || user.IsAuthenticated)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
