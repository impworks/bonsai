using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Utilities for working with the session.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Saves the object to session.
        /// </summary>
        public static void Set<T>(this ISession session, string key, T obj)
        {
            session.SetString(key, JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        /// Loads an object from the session.
        /// </summary>
        public static T Get<T>(this ISession session, string key)
        {
            var raw = session.GetString(key);
            return string.IsNullOrEmpty(raw)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(raw);
        }
    }
}
