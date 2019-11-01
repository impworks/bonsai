using System.Threading.Tasks;
using Bonsai.Code.Services;
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
        public AuthHandler(AppConfigService cfgProvider, UserManager<AppUser> userMgr)
        {
            _cfgProvider = cfgProvider;
            _userMgr = userMgr;
        }

        private readonly AppConfigService _cfgProvider;
        private readonly UserManager<AppUser> _userMgr;

        /// <summary>
        /// Checks the authorization if the config requires it.
        /// </summary>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthRequirement requirement)
        {
            var cfg = _cfgProvider.GetAppConfig();
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
