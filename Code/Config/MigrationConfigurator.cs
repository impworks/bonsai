using System.IO;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bonsai.Code.Config
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
