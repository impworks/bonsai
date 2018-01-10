using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Code.Services;
using Bonsai.Code.Tools;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Bonsai.Data.Utils.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bonsai.Code.Config
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        /// <summary>
        /// Registers all required services in the dependency injection container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            services.AddMvc()
                    .AddControllersAsServices()
                    .AddJsonOptions(opts =>
                    {
                        var convs = new List<JsonConverter>
                        {
                            new FuzzyDate.FuzzyDateJsonConverter(),
                            new FuzzyRange.FuzzyRangeJsonConverter()
                        };

                        convs.ForEach(opts.SerializerSettings.Converters.Add);

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
                opts.LowercaseUrls = true;
            });

            services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => new UrlHelper(x.GetService<IActionContextAccessor>().ActionContext));

            services.AddTransient<AppDbContext>();
            services.AddTransient<MarkdownService>();
            services.AddTransient<AppConfigService>();
            services.AddTransient<RelationsPresenterService>();
            services.AddTransient<PagePresenterService>();
            services.AddTransient<MediaPresenterService>();

            if (Environment.IsProduction())
            {
                services.Configure<MvcOptions>(opts =>
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                });
            }
        }

        /// <summary>
        /// Configures the web framework pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<AppDbContext>();
                context.EnsureDatabaseCreated();

                if(Environment.IsDevelopment())
                    context.EnsureSeeded();
            }

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            if (Environment.IsProduction())
            {
                app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
