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

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                    .AddFacebook(opts =>
                    {
                        opts.AppId = Configuration["Auth:Facebook:AppId"];
                        opts.AppSecret = Configuration["Auth:Facebook:AppSecret"];

                        foreach(var scope in new[] { "email", "user_birthday", "user_gender" })
                            opts.Scope.Add(scope);
                    })
                    .AddGoogle(opts =>
                    {
                        opts.ClientId = Configuration["Auth:Google:ClientId"];
                        opts.ClientSecret = Configuration["Auth:Google:ClientSecret"];

                        foreach(var scope in new[] { "email", "profile" })
                            opts.Scope.Add(scope);
                    });

            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = "/auth/login";
                opts.AccessDeniedPath = "/auth/login";
                opts.ReturnUrlParameter = "returnUrl";
            });
        }
    }
}
