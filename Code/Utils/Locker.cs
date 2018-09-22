using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Utility for locking based on a certain ID.
    /// </summary>
    public class Locker<T>
    {
        public Locker()
        {
            _locks = new ConcurrentDictionary<T, SemaphoreSlim>();
        }

        /// <summary>
        /// Lookup table for semaphores.
        /// </summary>
        private readonly ConcurrentDictionary<T, SemaphoreSlim> _locks;

        /// <summary>
        /// Waits for the resource to be free.
        /// </summary>
        public async Task WaitAsync(T id, CancellationToken token)
        {
            await _locks.GetOrAdd(id, x => new SemaphoreSlim(1, 1))
                        .WaitAsync(token);
        }

        /// <summary>
        /// Frees the occupied resource.
        /// </summary>
        public void Release(T id)
        {
            if(_locks.TryRemove(id, out var currLock))
                currLock.Release();
        }
    }
}
