using System.Collections.Generic;
using System.Linq;

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
    }
}
