using System.Net;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Logic.Validation;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Config;
using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Http.Features;
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
            services.Configure<FormOptions>(x =>
            {
                // actual value set in config via ConfigurableRequestSizeLimitFilter
                x.MemoryBufferThreshold = int.MaxValue;
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            services.AddNodeJS();
            services.Configure<OutOfProcessNodeJSServiceOptions>(opts => opts.TimeoutMS = -1);
            services.Configure<HttpNodeJSServiceOptions>(opts => opts.Version = HttpVersion.Version20);

            // common
            services.AddScoped<MarkdownService>();
            services.AddScoped<CacheService>();
            services.AddScoped<BonsaiConfigService>();

            services.AddSingleton<WorkerAlarmService>();
            services.AddSingleton<StartupService>();

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
            services.AddScoped<DynamicConfigManagerService>();
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
            services.AddScoped<IMediaHandler, PdfMediaHandler>();

            services.AddHostedService<MediaEncoderService>();
            services.AddHostedService<TreeLayoutService>();
        }
    }
}
