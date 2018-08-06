using System.Collections.Concurrent;
using System.Linq;
using Bonsai.Data;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Provides read-only configuration instance.
    /// </summary>
    public class AppConfigService
    {
        public AppConfigService(AppDbContext context)
        {
            _context = context;
            _config = new ConcurrentDictionary<string, AppConfig>();
        }

        private readonly AppDbContext _context;
        private static ConcurrentDictionary<string, AppConfig> _config;

        /// <summary>
        /// Returns the configuration instance.
        /// </summary>
        public AppConfig GetConfig()
        {
            return _config.GetOrAdd("default", x => LoadOrCreateConfig());
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
