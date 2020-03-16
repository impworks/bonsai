using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for IEnumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts the IEnumerable to a readonly list.
        /// </summary>
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
        {
            return source.ToList();
        }

        /// <summary>
        /// Converts an async enumerable to a list.
        /// </summary>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach(var elem in source)
                result.Add(elem);

            return result;
        }
    }
}
