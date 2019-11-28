using System.Collections.Concurrent;
using System.Linq;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.Extensions.Configuration;

namespace Bonsai.Code.Services.Config
{
    /// <summary>
    /// Provides read-only configuration instance.
    /// </summary>
    public class AppConfigService
    {
        public AppConfigService(AppDbContext context, IConfiguration cfg)
        {
            _context = context;
            _cfg = cfg;
            _config = new ConcurrentDictionary<string, AppConfig>();
        }

        private readonly AppDbContext _context;
        private readonly IConfiguration _cfg;
        private static ConcurrentDictionary<string, AppConfig> _config;

        /// <summary>
        /// Returns the configuration instance.
        /// </summary>
        public AppConfig GetAppConfig()
        {
            return _config.GetOrAdd("default", x => LoadOrCreateConfig());
        }

        /// <summary>
        /// Returns the configuration options from appsettings.json or env variables. 
        /// </summary>
        public StaticConfig GetStaticConfig()
        {
            return _cfg.Get<StaticConfig>();
        }

        /// <summary>
        /// Resets the currently loaded config.
        /// </summary>
        public void ResetCache()
        {
            _config.TryRemove("default", out _);
        }

        /// <summary>
        /// Loads the configuration instance from the database.
        /// </summary>
        private AppConfig LoadOrCreateConfig()
        {
            return _context.Config.First();
        }
    }
}
