using System.Linq;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Useful methods for working with strings.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// Returns the first non-empty string in the list.
        /// </summary>
        public static string Coalesce(params string[] items)
        {
            return items.FirstOrDefault(x => !string.IsNullOrEmpty(x));
        }
    }
}
