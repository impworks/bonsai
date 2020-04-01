using System.Collections.Concurrent;
using System.Linq;
using Bonsai.Data;
using Newtonsoft.Json;

namespace Bonsai.Code.Services.Config
{
    /// <summary>
    /// Provides read-only configuration instance.
    /// </summary>
    public class BonsaiConfigService
    {
        static BonsaiConfigService()
        {
            _config = new ConcurrentDictionary<string, DynamicConfig>();
        }

        public BonsaiConfigService(AppDbContext context, StaticConfig cfg)
        {
            _context = context;

            if(cfg.DemoMode == null)
                cfg.DemoMode = new DemoModeConfig();

            _cfg = cfg;
        }

        private readonly AppDbContext _context;
        private readonly StaticConfig _cfg;
        private static ConcurrentDictionary<string, DynamicConfig> _config;

        /// <summary>
        /// Returns the configuration instance.
        /// </summary>
        public DynamicConfig GetDynamicConfig()
        {
            return _config.GetOrAdd("default", x => LoadDynamicConfig());
        }

        /// <summary>
        /// Returns the configuration options from appsettings.json or env variables. 
        /// </summary>
        public StaticConfig GetStaticConfig()
        {
            return _cfg;
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
        private DynamicConfig LoadDynamicConfig()
        {
            var cfg = JsonConvert.DeserializeObject<DynamicConfig>(
                _context.DynamicConfig.First().Value
            );

            ApplyDefaults(cfg);

            return cfg;
        }

        /// <summary>
        /// Sets default values to properties (for backwards compatibility).
        /// </summary>
        private void ApplyDefaults(DynamicConfig cfg)
        {
            if (cfg.TreeRenderThoroughness == 0)
                cfg.TreeRenderThoroughness = 50;
        }
    }
}
