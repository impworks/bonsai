using System.IO;
using Bonsai.Code.Services.Config;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bonsai.Data.Utils
{
    /// <summary>
    /// Configures the design-time instance of the data context (for migrations).
    /// </summary>
    [UsedImplicitly]
    public class MigrationConfigurator: IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json")
                .Build()
                .Get<StaticConfig>()
                .ConnectionStrings;

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            if (config.UseEmbeddedDatabase)
                builder.UseSqlite(config.EmbeddedDatabase);
            else
                builder.UseNpgsql(config.Database);

            return new AppDbContext(builder.Options);
        }
    }
}
