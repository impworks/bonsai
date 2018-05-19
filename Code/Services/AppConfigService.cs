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
            _lock = new object();
        }

        private readonly AppDbContext _context;
        private readonly object _lock;
        private static AppConfig _config;

        /// <summary>
        /// Returns the configuration instance.
        /// </summary>
        public AppConfig GetConfig()
        {
            if (_config == null)
                lock (_lock)
                    if (_config == null)
                        _config = LoadOrCreateConfig();

            return _config;
        }

        /// <summary>
        /// Resets the currently loaded config.
        /// </summary>
        public void ResetCache()
        {
            lock(_lock)
                _config = null;
        }

        /// <summary>
        /// Loads the configuration instance from the database.
        /// </summary>
        private AppConfig LoadOrCreateConfig()
        {
            var existing = _context.Config.FirstOrDefault();
            if (existing != null)
                return existing;

            return new AppConfig
            {
                AllowGuests = true,
                Title = "Bonsai"
            };
        }
    }
}
