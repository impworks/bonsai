using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Auth;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Areas.Front.Logic.Relations;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Elastic;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Bonsai.Data.Utils.Seed;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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
using Nest;
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
                .AddUserSecrets<Startup>()
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
            // order is crucial
            ConfigureMvcServices(services);
            ConfigureDatabaseServices(services);
            ConfigureAuthServices(services);
            ConfigureElasticServices(services);
            ConfigureAutomapper(services);

            services.AddScoped<MarkdownService>();
            services.AddScoped<AppConfigService>();
            services.AddScoped<RelationsPresenterService>();
            services.AddScoped<PagePresenterService>();
            services.AddScoped<MediaPresenterService>();
            services.AddScoped<CalendarPresenterService>();
            services.AddScoped<SearchPresenterService>();
            services.AddScoped<AuthService>();

            services.AddScoped<DashboardPresenterService>();
            services.AddScoped<UserManagerService>();
            services.AddScoped<AppConfigManagerService>();
            services.AddScoped<PagesManagerService>();
            services.AddScoped<MediaManagerService>();
        }

        /// <summary>
        /// Configures the web framework pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<AppDbContext>();
                var elastic = scope.ServiceProvider.GetService<ElasticService>();

                context.EnsureDatabaseCreated();

                if(Environment.IsDevelopment())
                    SeedData.EnsureSeeded(context, elastic);
            }

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage()
                   .UseBrowserLink();
            }

            if (Environment.IsProduction())
            {
                app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
            }

            app.UseStaticFiles()
               .UseAuthentication()
               .UseSession()
               .UseMvc(routes => { routes.MapAreaRoute("admin", "Admin", "admin/{controller}/{action}/{id?}"); });
        }

        /// <summary>
        /// Configures and registers MVC-related services.
        /// </summary>
        private void ConfigureMvcServices(IServiceCollection services)
        {
            services.AddMvc()
                    .AddControllersAsServices()
                    .AddSessionStateTempDataProvider()
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
                opts.LowercaseUrls = false;
            });

            services.AddSession();

            services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => new UrlHelper(x.GetService<IActionContextAccessor>().ActionContext));

            if (Environment.IsProduction())
            {
                services.Configure<MvcOptions>(opts =>
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                });
            }
        }

        /// <summary>
        /// Configures the auth-related sessions.
        /// </summary>
        private void ConfigureAuthServices(IServiceCollection services)
        {
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(AuthRequirement.Name, p =>
                {
                    p.Requirements.Add(new AuthRequirement());
                });

                opts.AddPolicy(AdminAuthRequirement.Name, p => { p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Editor)); });
            });

            services.AddScoped<IAuthorizationHandler, AuthHandler>();

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                    .AddFacebook(opts =>
                    {
                        opts.AppId = Configuration["Auth:Facebook:AppId"];
                        opts.AppSecret = Configuration["Auth:Facebook:AppSecret"];

                        foreach(var scope in new [] { "email", "user_birthday", "user_gender" })
                            opts.Scope.Add(scope);
                    })
                    .AddGoogle(opts =>
                    {
                        opts.ClientId = Configuration["Auth:Google:ClientId"];
                        opts.ClientSecret = Configuration["Auth:Google:ClientSecret"];

                        foreach(var scope in new [] { "email", "profile" })
                            opts.Scope.Add(scope);
                    });

            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = "/auth/login";
                opts.AccessDeniedPath = "/auth/login";
                opts.ReturnUrlParameter = "returnUrl";
            });
        }

        /// <summary>
        /// Configures and registers database-related services.
        /// </summary>
        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(Configuration.GetConnectionString("Database")));

            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            SqlMapper.AddTypeHandler(new FuzzyDate.FuzzyDateTypeHandler());
            SqlMapper.AddTypeHandler(new FuzzyDate.NullableFuzzyDateTypeHandler());
            SqlMapper.AddTypeHandler(new FuzzyRange.FuzzyRangeTypeHandler());
            SqlMapper.AddTypeHandler(new FuzzyRange.NullableFuzzyRangeTypeHandler());
        }

        /// <summary>
        /// Registers ElasticSearch-related services.
        /// </summary>
        private void ConfigureElasticServices(IServiceCollection services)
        {
            var host = Configuration["ElasticSearch:Host"];
            var settings = new ConnectionSettings(new Uri(host)).DisableAutomaticProxyDetection()
                                                                .DisablePing();

            services.AddScoped(s => new ElasticClient(settings));
            services.AddScoped<ElasticService>();
        }

        /// <summary>
        /// Registers Automapper.
        /// </summary>
        private void ConfigureAutomapper(IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(x => x.IsClass
                                            && !x.IsAbstract
                                            && x.GetInterfaces().Any(y => y == typeof(IMapped))
                                            && x.GetMethods().Any(y => y.Name == nameof(IMapped.Configure) && y.DeclaringType == x))
                                .ToList();

            services.AddAutoMapper(opts =>
            {
                opts.CreateProfile("Default", p =>
                {
                    foreach (var type in types)
                    {
                        try
                        {
                            var t = (IMapped) Activator.CreateInstance(type);
                            t.Configure(p);
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                });
            });

            Mapper.AssertConfigurationIsValid();
        }
    }
}
