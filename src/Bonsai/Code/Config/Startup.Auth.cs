using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Areas.Front.Logic.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Configures the auth-related sessions.
        /// </summary>
        private void ConfigureAuthServices(IServiceCollection services)
        {
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(AuthRequirement.Name, p => p.Requirements.Add(new AuthRequirement()));
                opts.AddPolicy(AdminAuthRequirement.Name, p => p.Requirements.Add(new AdminAuthRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, AuthHandler>();
            services.AddScoped<IAuthorizationHandler, AdminAuthHandler>();

            var auth = services.AddAuthentication(IdentityConstants.ApplicationScheme);
            var authProvider = new AuthProviderService();
            authProvider.Initialize(Configuration, auth);
            services.AddSingleton(authProvider);

            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = "/auth/login";
                opts.AccessDeniedPath = "/auth/login";
                opts.ReturnUrlParameter = "returnUrl";
            });
        }
    }
}
