using Bonsai.Code.Services.Elastic;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Bonsai.Data.Utils.Seed;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
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
        /// Applies database migrations and seeds data.
        /// </summary>
        private void InitDatabase(IApplicationBuilder app)
        {
            using(var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<AppDbContext>();
                var elastic = scope.ServiceProvider.GetService<ElasticService>();

                context.EnsureDatabaseCreated();

                if(Environment.IsDevelopment())
                    SeedData.EnsureSeeded(context, elastic);
            }
        }
    }
}
