using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonsai.Areas.Admin.ViewModels.Common;
using Impworks.Utils.Format;

namespace Bonsai.Areas.Admin.Utils
{
    /// <summary>
    /// Helper methods for automatically managing ListRequests.
    /// </summary>
    public static class ListRequestHelper
    {
        /// <summary>
        /// Gets all arbitrary values for the request.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetValues(ListRequestVM vm)
        {
            var dict = new List<KeyValuePair<string, string>>();

            if (vm == null)
                return dict;

            var props = vm.GetType().GetProperties();
            foreach (var prop in props)
            {
                var propType = prop.PropertyType;
                var value = prop.GetValue(vm);

                if (value == null)
                    continue;

                var defValue = propType.IsValueType && Nullable.GetUnderlyingType(propType) == null
                    ? Activator.CreateInstance(propType)
                    : null;

                if (value.Equals(defValue))
                    continue;

                var isEnumerable = propType.IsArray
                                   || (propType != typeof(string) && propType.GetInterfaces().Contains(typeof(IEnumerable)));
                if (isEnumerable)
                {
                    foreach (object elem in (dynamic) value)
                        Add(prop.Name, elem);
                }
                else
                {
                    Add(prop.Name, value);
                }
            }

            return dict;

            void Add(string propName, object value)
            {
                var str = value is IConvertible fmt
                    ? fmt.ToInvariantString()
                    : value.ToString();

                dict.Add(new KeyValuePair<string, string>(propName, str));
            }
        }

        /// <summary>
        /// Returns the URL for current request.
        /// </summary>
        public static string GetUrl(string url, ListRequestVM request)
        {
            var args = GetValues(request);
            var strArgs = args.Select(x => HttpUtility.UrlEncode(x.Key) + "=" + HttpUtility.UrlEncode(x.Value));
            return url + "?" + string.Join("&", strArgs);
        }
    }
}
