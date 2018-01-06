using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bonsai.Data.Utils
{
    /// <summary>
    /// Configures the design-time instance of the data context (for migrations).
    /// </summary>
    public class MigrationConfigurator: IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            return new AppDbContext(builder.Options);
        }
    }
}
