using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Hosting;
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
            var demoCfg = Configuration.DemoMode ?? new DemoModeConfig(); // all false

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

            if (demoCfg.Enabled)
            {
                if (demoCfg.ClearOnStartup)
                {
                    tasks = tasks.ContinueWith(
                        "DatabaseClear",
                        "Очистка базы данных",
                        () => SeedData.ClearPreviousDataAsync(sp.GetService<AppDbContext>())
                    );
                }

                if (demoCfg.CreateDefaultPages || demoCfg.CreateDefaultAdmin)
                {
                    tasks = tasks.ContinueWith(
                        "DatabaseSeed",
                        "Подготовка тестовых данных",
                        async () =>
                        {
                            if (demoCfg.CreateDefaultPages)
                                await SeedData.EnsureSampleDataSeededAsync(sp.GetService<AppDbContext>());

                            if (demoCfg.CreateDefaultAdmin)
                                await SeedData.EnsureDefaultUserCreatedAsync(sp.GetService<UserManager<AppUser>>());
                        });
                }
            }

            tasks = tasks.ContinueWith(
                "FullTextIndexInit",
                "Подготовка поискового индекса",
                async () =>
                {
                    var search = sp.GetService<ISearchEngine>();
                    var db = sp.GetService<AppDbContext>();

                    await search.InitializeAsync();

                    var pages = await db.Pages.Include(x => x.Aliases).ToListAsync();
                    foreach (var page in pages)
                        await search.AddPageAsync(page);
                });

            tasks = tasks.ContinueWith("CheckMissingMedia", "", () => CheckMissingMediaAsync(sp));
            
            tasks.ContinueWith("Finalize", "", async () => scope.Dispose());
        }

        /// <summary>
        /// Checks if the media folder is not mounted correctly.
        /// </summary>
        private async Task CheckMissingMediaAsync(IServiceProvider sp)
        {
            var db = sp.GetService<AppDbContext>();
            var env = sp.GetService<IWebHostEnvironment>();
            
            if (!(await db.Media.AnyAsync()))
                return;

            var path = Path.Combine(env.WebRootPath, "media");
            if (!Directory.Exists(path) || !Directory.EnumerateFiles(path).Any())
                Logger.Error("The 'media' directory is missing. Make sure it is mounted properly.");
        }
    }
}
