using System;
using Bonsai.Code.Services.Elastic;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers ElasticSearch-related services.
        /// </summary>
        private void ConfigureElasticServices(IServiceCollection services)
        {
            var host = Configuration.ElasticSearch.Host;
            var settings = new ConnectionSettings(new Uri(host)).DisableAutomaticProxyDetection()
                                                                .DisablePing();

            services.AddScoped(s => new ElasticClient(settings));
            services.AddScoped<ElasticService>();
        }
    }
}
