using System.Collections.Generic;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Various dictionary helpers.
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// Attempts to return a value from the dictionary.
        /// </summary>
        public static TVal? TryGetNullableValue<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key)
            where TVal : struct
        {
            return dict.TryGetValue(key, out var result) ? result : (TVal?) null;
        }

        /// <summary>
        /// Attempts to return a value from the dictionary.
        /// </summary>
        public static TVal TryGetValue<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key)
        {
            return dict.TryGetValue(key, out var result) ? result : default(TVal);
        }
    }
}
