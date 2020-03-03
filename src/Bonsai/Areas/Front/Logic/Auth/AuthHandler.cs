using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Authorization handler for requiring login depending on the config.
    /// </summary>
    public class AuthHandler: AuthorizationHandler<AuthRequirement>
    {
        public AuthHandler(BonsaiConfigService cfgProvider, UserManager<AppUser> userMgr)
        {
            _cfgProvider = cfgProvider;
            _userMgr = userMgr;
        }

        private readonly BonsaiConfigService _cfgProvider;
        private readonly UserManager<AppUser> _userMgr;

        /// <summary>
        /// Checks the authorization if the config requires it.
        /// </summary>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthRequirement requirement)
        {
            var cfg = _cfgProvider.GetDynamicConfig();
            if (cfg.AllowGuests)
            {
                context.Succeed(requirement);
                return;
            }

            var user = await _userMgr.GetUserAsync(context.User);
            if(user?.IsValidated == true)
                context.Succeed(requirement);
        }
    }
}
