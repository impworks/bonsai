using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using Npgsql;

namespace Bonsai.Data.Utils
{
    /// <summary>
    /// Extensions methods for configuring and seeding the database data.
    /// </summary>
    public static class AppDbContextExtensions
    {
        /// <summary>
        /// Applies migrations and applies migrations.
        /// </summary>
        public static async Task EnsureDatabaseCreatedAsync(this AppDbContext context)
        {
            if(!await context.IsMigratedAsync())
                await context.Database.MigrateAsync();
        }

        /// <summary>
        /// Checks if there are no pending migrations.
        /// </summary>
        private static async Task<bool> IsMigratedAsync(this AppDbContext context)
        {
            var applied = await context.GetService<IHistoryRepository>().GetAppliedMigrationsAsync();
            var total = context.GetService<IMigrationsAssembly>().Migrations;
            return !total.Select(x => x.Key).Except(applied.Select(x => x.MigrationId)).Any();
        }

        /// <summary>
        /// Creates a new connection from existing context.
        /// </summary>
        public static IDbConnection GetConnection(this AppDbContext context)
        {
            return new NpgsqlConnection(context.Database.GetDbConnection().ConnectionString);
        }

        /// <summary>
        /// Adds required records (identity, config, etc.).
        /// </summary>
        public static async Task EnsureSystemItemsCreatedAsync(this AppDbContext db)
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
    }
}
