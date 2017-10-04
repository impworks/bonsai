using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Config
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting(opts =>
            {
                opts.AppendTrailingSlash = false;
                opts.LowercaseUrls = true;
            });

#if RELEASE
            services.Configure<MvcOptions>(opts =>
            {
                opts.Filters.Add(new RequireHttpsAttribute());
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

#if RELEASE
            app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
#endif

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
