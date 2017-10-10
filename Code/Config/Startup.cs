using Bonsai.Code.Services;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            services.AddMvc().AddControllersAsServices();
            services.AddRouting(opts =>
            {
                opts.AppendTrailingSlash = false;
                opts.LowercaseUrls = true;
            });

            services.AddTransient<AppDbContext>();
            services.AddTransient<AppConfigService>();

            if (Environment.IsProduction())
            {
                services.Configure<MvcOptions>(opts =>
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                });
            }
        }

        public void Configure(IApplicationBuilder app)
        {
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
