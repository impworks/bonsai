using Bonsai.Code.Services;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Search;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Bonsai.Data.Utils.Seed;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(Configuration.ConnectionStrings.Database));

            services.AddIdentity<AppUser, IdentityRole>(o =>
                    {
                        o.Password.RequireDigit = false;
                        o.Password.RequireLowercase = false;
                        o.Password.RequireUppercase = false;
                        o.Password.RequireNonAlphanumeric = false;
                        o.Password.RequiredLength = 6;
                        o.Password.RequiredUniqueChars = 1;
                    })
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
            var startupService = app.ApplicationServices.GetService<StartupService>();
            
            var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var sp = scope.ServiceProvider;
            var seedConfig = Configuration.SeedData ?? new SeedDataConfig(); // all false

            var tasks = startupService.RegisterStartupTask(
                "DatabaseMigrate",
                "Подготовка базы",
                async () =>
                {
                    var db = sp.GetService<AppDbContext>();
                    await db.EnsureDatabaseCreatedAsync();
                    await db.EnsureSystemItemsCreatedAsync();
                }
            );

            if (seedConfig.ClearAll)
            {
                tasks = tasks.ContinueWith(
                    "DatabaseClear",
                    "Очистка базы данных",
                    () => SeedData.ClearPreviousDataAsync(sp.GetService<AppDbContext>())
                );
            }

            if (seedConfig.Enable)
            {
                tasks = tasks.ContinueWith(
                    "DatabaseSeed",
                    "Подготовка тестовых данных",
                    () => SeedData.EnsureSampleDataSeededAsync(sp.GetService<AppDbContext>())
                );
            }

            tasks = tasks.ContinueWith(
                "FullTextIndexInit",
                "Подготовка поискового индекса",
                async () =>
                {
                    var search = sp.GetService<ISearchEngine>();
                    var db = sp.GetService<AppDbContext>();

                    await search.InitializeAsync();

                    var pages = await db.Pages.ToListAsync();
                    foreach (var page in pages)
                        await search.AddPageAsync(page);
                });

            tasks.ContinueWith("Finalize", "", async () => scope.Dispose());
        }
    }
}
