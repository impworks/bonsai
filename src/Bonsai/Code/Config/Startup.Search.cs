using Bonsai.Code.Services.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers fulltext search-related services.
        /// </summary>
        private void ConfigureSearchServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration.ElasticSearch);
            services.AddScoped<ISearchEngine, ElasticService>();
        }
    }
}
