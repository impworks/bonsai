using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;

namespace Bonsai.Data.Utils;

/// <summary>
/// Extensions methods for configuring and seeding the database data.
/// </summary>
public static class AppDbContextHelper
{
    /// <summary>
    /// Ensures that the database is properly migrated and populated.
    /// </summary>
    public static async Task UpdateDatabaseAsync(AppDbContext context)
    {
        await EnsureDatabaseCreatedAsync(context);
        await EnsureSystemItemsCreatedAsync(context);
        await EnsureTitlesNormalizedAsync(context);
    }

    /// <summary>
    /// Applies migrations and applies migrations.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(AppDbContext context)
    {
        var applied = await context.GetService<IHistoryRepository>().GetAppliedMigrationsAsync();
        var total = context.GetService<IMigrationsAssembly>().Migrations;
        var isMigrated = !total.Select(x => x.Key).Except(applied.Select(x => x.MigrationId)).Any();

        if(!isMigrated)
            await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Adds required records (identity, config, etc.).
    /// </summary>
    private static async Task EnsureSystemItemsCreatedAsync(AppDbContext db)
    {
        if(!db.Roles.Any())
        {
            db.Roles.AddRange(
                EnumHelper.GetEnumValues<UserRole>()
                          .Select(name => new IdentityRole { Name = name.ToString(), NormalizedName = name.ToString().ToUpper() })
            );
        }

        if(!db.DynamicConfig.Any())
        {
            db.DynamicConfig.Add(new DynamicConfigWrapper
            {
                Id = Guid.NewGuid(),
                Value = JsonConvert.SerializeObject(
                    new DynamicConfig
                    {
                        Title = "Bonsai",
                        AllowGuests = false,
                        AllowRegistration = true
                    }
                )
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Applies normalized names to pages and media.
    /// </summary>
    private static async Task EnsureTitlesNormalizedAsync(AppDbContext db)
    {
        var pages = await db.Pages.Where(x => x.NormalizedTitle == null).ToListAsync();
        if (pages.Any())
        {
            foreach (var p in pages)
                p.NormalizedTitle = PageHelper.NormalizeTitle(p.Title);

            await db.SaveChangesAsync();
        }

        var media = await db.Media.Where(x => x.NormalizedTitle == null).ToListAsync();
        if (media.Any())
        {
            foreach (var p in media)
                p.NormalizedTitle = PageHelper.NormalizeTitle(p.Title);

            await db.SaveChangesAsync();
        }

        var aliases = await db.PageAliases.Where(x => x.NormalizedTitle == null).ToListAsync();
        if (aliases.Any())
        {
            foreach (var a in aliases)
                a.NormalizedTitle = PageHelper.NormalizeTitle(a.Title);

            await db.SaveChangesAsync();
        }
    }
}