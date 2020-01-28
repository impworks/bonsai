using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Utilities for working with the session.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Saves the object to session using default name.
        /// </summary>
        public static void Set<T>(this ISession session, T obj)
        {
            session.Set(DefaultName<T>(), obj);
        }

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
        public static T Get<T>(this ISession session, string key = null)
        {
            var raw = session.GetString(key ?? DefaultName<T>());
            return string.IsNullOrEmpty(raw)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(raw);
        }

        /// <summary>
        /// Removes the object using default name.
        /// </summary>
        public static void Remove<T>(this ISession session)
        {
            session.Remove(DefaultName<T>());
        }

        /// <summary>
        /// Returns the default name for a resource.
        /// </summary>
        private static string DefaultName<T>()
        {
            return typeof(T).FullName;
        }
    }
}
