using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Logic.Validation;
using Bonsai.Areas.Admin.Logic.Workers;
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
        private void ConfigureAppServices(IServiceCollection services)
        {
            // common
            services.AddScoped<MarkdownService>();
            services.AddScoped<AppConfigService>();
            services.AddScoped<CacheService>();
            services.AddScoped<WorkerAlarmService>();

            // frontend
            services.AddScoped<RelationsPresenterService>();
            services.AddScoped<PagePresenterService>();
            services.AddScoped<MediaPresenterService>();
            services.AddScoped<CalendarPresenterService>();
            services.AddScoped<SearchPresenterService>();
            services.AddScoped<TreePresenterService>();
            services.AddScoped<AuthService>();

            // admin
            services.AddScoped<PageValidator>();
            services.AddScoped<RelationValidator>();
            services.AddScoped<DashboardPresenterService>();
            services.AddScoped<UsersManagerService>();
            services.AddScoped<AppConfigManagerService>();
            services.AddScoped<PagesManagerService>();
            services.AddScoped<RelationsManagerService>();
            services.AddScoped<MediaManagerService>();
            services.AddScoped<ChangesetsManagerService>();
            services.AddScoped<SuggestService>();
            services.AddScoped<NotificationsService>();

            services.AddScoped<IChangesetRenderer, MediaChangesetRenderer>();
            services.AddScoped<IChangesetRenderer, PageChangesetRenderer>();
            services.AddScoped<IChangesetRenderer, RelationChangesetRenderer>();

            services.AddScoped<IMediaHandler, PhotoMediaHandler>();
            services.AddScoped<IMediaHandler, VideoMediaHandler>();

            services.AddHostedService<MediaEncoderService>();
            services.AddHostedService<TreeLayoutService>();
        }
    }
}
