using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Code.Utils;
using Newtonsoft.Json;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// In-memory page cache service.
    /// </summary>
    public class CacheService: IDisposable
    {
        #region Constructor

        static CacheService()
        {
            _locks = new Locker<(Type type, string id)>();
            _cache = new ConcurrentDictionary<(Type type, string id), string>();
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public CacheService()
        {
            _cts = new CancellationTokenSource();
        }

        #endregion

        #region Fields

        private CancellationTokenSource _cts;

        private static readonly Locker<(Type type, string id)> _locks;
        private static readonly ConcurrentDictionary<(Type type, string id), string> _cache;
        private static readonly JsonSerializerSettings _jsonSettings;

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the page's contents to the cache.
        /// </summary>
        public async Task<T> GetOrAddAsync<T>(string id, Func<Task<T>> getter)
        {
            var key = (typeof(T), id);
            await _locks.WaitAsync(key, _cts.Token);
            try
            {
                if (_cache.ContainsKey(key))
                    return JsonConvert.DeserializeObject<T>(_cache[key], _jsonSettings);

                var result = await getter();
                _cache.TryAdd(key, JsonConvert.SerializeObject(result, _jsonSettings));

                return result;
            }
            finally
            {
                _locks.Release(key);
            }
        }

        /// <summary>
        /// Removes all entries of the specified type from the caching service.
        /// </summary>
        public void Remove<T>()
        {
            // todo: a better approach?
            var type = typeof(T);
            var stales = _cache.Keys.Where(x => x.type == type);

            foreach (var stale in stales)
                _cache.TryRemove(stale, out _);
        }

        /// <summary>
        /// Removes the entry from the caching service.
        /// </summary>
        public void Remove<T>(string id)
        {
            _cache.TryRemove((typeof(T), id), out _);
        }

        /// <summary>
        /// Clears the entire cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            _cts.Cancel();
        }

        #endregion
    }
}
