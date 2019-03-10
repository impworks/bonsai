using System;
using System.Data;
using System.Linq;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
        public static void EnsureDatabaseCreated(this AppDbContext context)
        {
            if(!context.IsMigrated())
                context.Database.Migrate();
        }

        /// <summary>
        /// Checks if there are no pending migrations.
        /// </summary>
        private static bool IsMigrated(this AppDbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                                 .GetAppliedMigrations()
                                 .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                               .Migrations
                               .Select(m => m.Key);

            return !total.Except(applied).Any();
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
        public static void EnsureSystemItemsCreated(this AppDbContext db)
        {
            void AddRole(string name) => db.Roles.Add(new IdentityRole { Name = name, NormalizedName = name.ToUpper() });

            if(!db.Roles.Any())
            {
                foreach(var role in EnumHelper.GetEnumValues<UserRole>())
                    AddRole(role.ToString());
            }

            if(!db.Config.Any())
            {
                db.Config.Add(new AppConfig
                {
                    Id = Guid.NewGuid(),
                    Title = "Bonsai",
                    AllowGuests = false
                });
            }

            db.SaveChanges();
        }
    }
}
