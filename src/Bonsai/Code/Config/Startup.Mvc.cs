using System.Collections.Generic;
using Bonsai.Code.Infrastructure.Attributes;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Date;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Configures and registers MVC-related services.
        /// </summary>
        private void ConfigureMvcServices(IServiceCollection services)
        {
            services.AddSingleton(p => Configuration);
            services.AddSingleton(p => Log.Logger);

            services.AddMvc()
                    .AddControllersAsServices()
                    .AddSessionStateTempDataProvider()
                    .AddRazorRuntimeCompilation()
                    .AddNewtonsoftJson(opts =>
                    {
                        var convs = new List<JsonConverter>
                        {
                            new FuzzyDate.FuzzyDateJsonConverter(),
                            new FuzzyRange.FuzzyRangeJsonConverter()
                        };

                        convs.ForEach(x => opts.SerializerSettings.Converters.Add(x));

                        JsonConvert.DefaultSettings = () =>
                        {
                            var settings = new JsonSerializerSettings();
                            convs.ForEach(settings.Converters.Add);
                            return settings;
                        };
                    });

            services.AddRouting(opts =>
            {
                opts.AppendTrailingSlash = false;
                opts.LowercaseUrls = false;
            });

            services.AddSession();

            services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x =>
            {
                var httpAcc = x.GetService<IHttpContextAccessor>();
                var urlFactory = x.GetService<IUrlHelperFactory>();
                var actionCtx = new ActionContext(httpAcc.HttpContext, httpAcc.HttpContext.GetRouteData(), new ActionDescriptor());
                return urlFactory.GetUrlHelper(actionCtx);
            });
            services.AddScoped<ViewRenderService>();
            services.AddScoped(x => Configuration);

            services.AddScoped<ConfigurableRequestSizeLimitFilter>();

            if(Configuration.WebServer.RequireHttps)
            {
                services.Configure<MvcOptions>(opts =>
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                });
            }
        }
    }
}
