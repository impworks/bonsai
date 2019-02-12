using System;
using System.Globalization;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        /// <summary>
        /// Registers all required services in the dependency injection container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // order is crucial
            ConfigureMvcServices(services);
            ConfigureDatabaseServices(services);
            ConfigureAuthServices(services);
            ConfigureElasticServices(services);
            ConfigureAutomapper(services);
            ConfigureAppServices(services);
        }

        /// <summary>
        /// Configures the web framework pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseBrowserLink();
            }

            if (Configuration["WebServer:RequireHttps"].TryParse<bool>())
            {
                app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
            }

            if (Configuration["Debug:DetailedExceptions"].TryParse<bool>())
            {
                app.UseDeveloperExceptionPage();
            }

            InitDatabase(app);

            var culture = CultureInfo.GetCultureInfo("ru-RU");

            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
            };
            forwardedHeadersOptions.KnownProxies.Clear();
            forwardedHeadersOptions.KnownNetworks.Clear();
            
            app.UseForwardedHeaders(forwardedHeadersOptions)
               .UseStatusCodePagesWithReExecute("/error/{0}")
               .UseStaticFiles()
               .UseAuthentication()
               .UseSession()
               .UseRequestLocalization(new RequestLocalizationOptions
               {
                   DefaultRequestCulture = new RequestCulture(culture),
                   SupportedCultures = new [] { culture },
                   SupportedUICultures = new [] { culture }
               })
               .UseMvc(routes => { routes.MapAreaRoute("admin", "Admin", "admin/{controller}/{action}/{id?}"); });
        }
    }
}
