using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Data.Utils
{
    public static class DatabaseReplicator
    {
        /// <summary>
        /// Checks if there is data in the original database which needs to be replicated to the new embedded one.
        /// </summary>
        public static bool IsReplicationPending(ConnectionStringsConfig config)
        {
            if (config.UseEmbeddedDatabase == false || string.IsNullOrEmpty(config.Database) || string.IsNullOrEmpty(config.EmbeddedDatabase))
                return false;

            var pathMatch = Regex.Match(
                config.EmbeddedDatabase,
                "Data Source=(?<path>.+)(;|$)",
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            if (pathMatch.Success == false)
                return false;

            var path = pathMatch.Groups["path"].Value;
            if (path == ":memory:")
                return false;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(fullPath))
                return false;

            var dir = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(dir);

            return true;
        }

        /// <summary>
        /// Replicates data from standalone to the separate database.
        /// </summary>
        public static async Task ReplicateAsync(ConnectionStringsConfig config)
        {
            var oldCtx = GetContext(false, config);
            await oldCtx.EnsureDatabaseCreatedAsync();
            await oldCtx.EnsureSystemItemsCreatedAsync();

            var newCtx = GetContext(true, config);
            await newCtx.EnsureDatabaseCreatedAsync();

            await newCtx.Database.ExecuteSqlAsync($"PRAGMA foreign_keys = 0");

            await ReplicateEntriesAsync(x => x.Changes);
            await ReplicateEntriesAsync(x => x.DynamicConfig);
            await ReplicateEntriesAsync(x => x.JobStates);
            await ReplicateEntriesAsync(x => x.LivingBeingOverviews);
            await ReplicateEntriesAsync(x => x.Media);
            await ReplicateEntriesAsync(x => x.MediaTags);
            await ReplicateEntriesAsync(x => x.Pages);
            await ReplicateEntriesAsync(x => x.PageAliases);
            await ReplicateEntriesAsync(x => x.PageDrafts);
            await ReplicateEntriesAsync(x => x.PageReferences);
            await ReplicateEntriesAsync(x => x.Relations);
            await ReplicateEntriesAsync(x => x.Roles);
            await ReplicateEntriesAsync(x => x.RoleClaims);
            await ReplicateEntriesAsync(x => x.TreeLayouts);
            await ReplicateEntriesAsync(x => x.Users);
            await ReplicateEntriesAsync(x => x.UserClaims);
            await ReplicateEntriesAsync(x => x.UserLogins);
            await ReplicateEntriesAsync(x => x.UserRoles);
            await ReplicateEntriesAsync(x => x.UserTokens);

            await newCtx.SaveChangesAsync();

            async Task ReplicateEntriesAsync<T>(Func<AppDbContext, DbSet<T>> setGetter) where T: class
            {
                var items = await setGetter(oldCtx).AsNoTracking().ToListAsync();
                setGetter(newCtx).AddRange(items);
            }
        }

        /// <summary>
        /// Creates an instance of the context.
        /// </summary>
        private static AppDbContext GetContext(bool isEmbedded, ConnectionStringsConfig config)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            if (isEmbedded)
                builder.UseSqlite(config.EmbeddedDatabase);
            else
                builder.UseNpgsql(config.Database);
            return new AppDbContext(builder.Options);
        }
    }
}
