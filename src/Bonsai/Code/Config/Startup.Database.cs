using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Services.Search;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Bonsai.Data.Utils.Seed;
using Bonsai.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Code.Config;

public partial class Startup
{
    /// <summary>
    /// Configures and registers database-related services.
    /// </summary>
    private void ConfigureDatabaseServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var cfg = Configuration.ConnectionStrings;
            if (cfg.UseEmbeddedDatabase)
                opts.UseSqlite(cfg.EmbeddedDatabase);
            else
                opts.UseNpgsql(cfg.Database);
        });

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

        if (DatabaseReplicator.IsReplicationPending(Configuration.ConnectionStrings))
        {
            startupService.AddTask(
                "DatabaseReplicate",
                Texts.Startup_Task_DatabaseReplication,
                () => DatabaseReplicator.ReplicateAsync(Configuration.ConnectionStrings)
            );
        }
        else
        {
            startupService.AddTask(
                "DatabaseMigrate",
                Texts.Startup_Task_DatabaseMigration,
                async () =>
                {
                    var db = sp.GetService<AppDbContext>();
                    await AppDbContextHelper.UpdateDatabaseAsync(db);
                }
            );
        }

        if (demoCfg.Enabled)
        {
            if (demoCfg.ClearOnStartup)
            {
                startupService.AddTask(
                    "DatabaseClear",
                    Texts.Startup_Task_DatabaseCleanup,
                    () => SeedData.ClearPreviousDataAsync(sp.GetService<AppDbContext>())
                );
            }

            if (demoCfg.CreateDefaultPages || demoCfg.CreateDefaultAdmin)
            {
                startupService.AddTask(
                    "DatabaseSeed",
                    Texts.Startup_Task_TestDataSeeding,
                    async () =>
                    {
                        if (demoCfg.CreateDefaultPages)
                            await SeedData.EnsureSampleDataSeededAsync(sp.GetService<AppDbContext>());

                        if (demoCfg.CreateDefaultAdmin)
                            await SeedData.EnsureDefaultUserCreatedAsync(sp.GetService<UserManager<AppUser>>());
                    });
            }

            startupService.AddTask(
                "PrepareConfig",
                Texts.Startup_Task_Configuration,
                async () =>
                {
                    var db = sp.GetRequiredService<AppDbContext>();
                    var wrapper = await db.DynamicConfig.FirstAsync();
                    var cfg = JsonConvert.DeserializeObject<DynamicConfig>(wrapper.Value);
                    cfg.TreeKinds = TreeKind.FullTree | TreeKind.CloseFamily | TreeKind.Ancestors | TreeKind.Descendants;
                    wrapper.Value = JsonConvert.SerializeObject(cfg);
                    await db.SaveChangesAsync();
                }
            );
        }

        startupService.AddTask(
            "FullTextIndexInit",
            Texts.Startup_Task_SearchIndexPreparation,
            async () =>
            {
                var search = sp.GetService<ISearchEngine>();
                var db = sp.GetService<AppDbContext>();

                await search.InitializeAsync();

                var pages = await db.Pages.Include(x => x.Aliases).ToListAsync();
                foreach (var page in pages)
                    await search.AddPageAsync(page);
            }
        );
            
        startupService.AddTask(
            "BuildPageReferences",
            Texts.Startup_Task_ReferenceDetection,
            () => BuildPageReferences(sp)
        );

        startupService.AddTask(
            "InitTree",
            Texts.Startup_Task_TreeBuilding,
            async () =>
            {
                var db = sp.GetService<AppDbContext>();
                if (await db.TreeLayouts.AnyAsync())
                    return;

                var jobs = sp.GetService<IBackgroundJobService>();
                await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());
            }
        );

        startupService.AddTask("CheckMissingMedia", "", () => CheckMissingMediaAsync(sp));
            
        startupService.AddTask("Finalize", "", async () => scope.Dispose());

        startupService.RunStartupTasks();
    }

    /// <summary>
    /// Checks if the media folder is not mounted correctly.
    /// </summary>
    private async Task CheckMissingMediaAsync(IServiceProvider sp)
    {
        var db = sp.GetService<AppDbContext>();
        var env = sp.GetService<IWebHostEnvironment>();
            
        if (await db.Media.AnyAsync() == false)
            return;

        var path = Path.Combine(env.WebRootPath, "media");
        if (!Directory.Exists(path) || !Directory.EnumerateFiles(path).Any())
        {
            var logger = sp.GetService<ILogger>();
            logger.Error("The 'media' directory is missing. Make sure it is mounted properly.");
        }
    }

    /// <summary>
    /// Parses all pages, finding cross links in descriptions. 
    /// </summary>
    private async Task BuildPageReferences(IServiceProvider sp)
    {
        var db = sp.GetService<AppDbContext>();
        if (await db.PageReferences.AnyAsync())
            return;

        var pages = await db.Pages.Select(x => new {x.Id, x.Key, x.Description})
                            .ToDictionaryAsync(x => x.Key, x => x);

        foreach (var p in pages.Values)
        {
            var links = MarkdownService.GetPageReferences(p.Description);
            foreach (var link in links)
            {
                if (!pages.TryGetValue(link, out var linkRef))
                    continue;

                db.PageReferences.Add(new PageReference
                {
                    Id = Guid.NewGuid(),
                    SourceId = p.Id,
                    DestinationId = linkRef.Id
                });
            }
        }

        await db.SaveChangesAsync();
    }
}