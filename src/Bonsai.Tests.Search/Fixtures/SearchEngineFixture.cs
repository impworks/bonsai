using System.IO;
using System.Threading.Tasks;
using Bonsai.Code.Services.Search;
using Bonsai.Data;
using Bonsai.Data.Utils.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bonsai.Tests.Search.Fixtures
{
    /// <summary>
    /// Shared context for all fulltext search tests.
    /// </summary>
    public class SearchEngineFixture: IAsyncLifetime
    {
        public SearchEngineFixture()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                       .UseInMemoryDatabase("default")
                       .Options;

            Db = new AppDbContext(opts);
            // Search = new ElasticService(new ElasticSearchConfig { Host = "http://localhost:9200", IndexName = "test_pages" });
            Search = new LuceneNetService();
        }

        public readonly AppDbContext Db;
        public readonly ISearchEngine Search;

        /// <summary>
        /// Initializes the search index with test data.
        /// </summary>
        public async Task InitializeAsync()
        {
            var rootPath = Path.GetFullPath(Directory.GetCurrentDirectory());
            var wwwPath = Path.Combine(rootPath, "wwwroot");
            if(Directory.Exists(wwwPath))
                Directory.Delete(wwwPath, true);
            await Search.ClearDataAsync();

            var seedPath = Path.Combine(rootPath, "..", "..", "..", "..", "Bonsai", "Data", "Utils", "Seed");
            await SeedData.EnsureSampleDataSeededAsync(Db, seedPath);

            await foreach (var page in Db.Pages)
                await Search.AddPageAsync(page);
        }

        /// <summary>
        /// Releases the in-memory testing database.
        /// </summary>
        public async Task DisposeAsync()
        {
            await Db.DisposeAsync();
        }
    }
}
