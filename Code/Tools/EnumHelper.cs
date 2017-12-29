using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Bonsai.Code.Tools
{
    /// <summary>
    /// Various extension / helper methods for enum values.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Cached description values for all values of known enums.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, object> DescriptionCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Returns the lookup of enum values and readable descriptions.
        /// </summary>
        public static IReadOnlyDictionary<T, string> GetEnumDescriptions<T>()
            where T: struct
        {
            var type = typeof(T);
            var lookup = DescriptionCache.GetOrAdd(type, t =>
            {
                var flags = BindingFlags.DeclaredOnly
                            | BindingFlags.Static
                            | BindingFlags.Public
                            | BindingFlags.GetField;

                return type.GetFields(flags)
                           .ToDictionary(
                               x => (T) x.GetRawConstantValue(),
                               x => x.GetCustomAttribute<DescriptionAttribute>()?.Description
                                    ?? x.GetRawConstantValue().ToString()
                           );
            });

            return (IReadOnlyDictionary<T, string>) lookup;
        }

        /// <summary>
        /// Returns a readable description of a single enum value.
        /// </summary>
        public static string GetEnumDescription<T>(this T enumValue)
            where T : struct
        {
            return GetEnumDescriptions<T>()[enumValue];
        }
    }
}
