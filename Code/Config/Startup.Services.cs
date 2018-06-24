using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Code.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Register application-level services.
        /// </summary>
        public void ConfigureAppServices(IServiceCollection services)
        {
            // common
            services.AddScoped<MarkdownService>();
            services.AddScoped<AppConfigService>();

            // frontend
            services.AddScoped<RelationsPresenterService>();
            services.AddScoped<PagePresenterService>();
            services.AddScoped<MediaPresenterService>();
            services.AddScoped<CalendarPresenterService>();
            services.AddScoped<SearchPresenterService>();
            services.AddScoped<AuthService>();

            // admin
            services.AddScoped<DashboardPresenterService>();
            services.AddScoped<UserManagerService>();
            services.AddScoped<AppConfigManagerService>();
            services.AddScoped<PagesManagerService>();
            services.AddScoped<MediaManagerService>();
        }
    }
}
