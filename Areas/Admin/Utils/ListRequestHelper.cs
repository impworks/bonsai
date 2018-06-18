using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bonsai.Areas.Admin.ViewModels.Common;

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

            if (vm != null)
            {
                var props = vm.GetType().GetProperties();

                foreach (var prop in props)
                {
                    var pt = prop.PropertyType;
                    var defValue = pt.IsValueType && Nullable.GetUnderlyingType(pt) == null ? Activator.CreateInstance(pt) : null;
                    var value = prop.GetValue(vm);

                    if (value == null || value.Equals(defValue))
                        continue;

                    var isEnumerable = pt.IsArray || pt.GetInterfaces().Contains(typeof(IEnumerable));
                    if (isEnumerable)
                    {
                        foreach(object elem in (dynamic) value)
                            dict.Add(new KeyValuePair<string, string>(prop.Name, elem.ToString()));
                    }
                    else
                    {
                        dict.Add(new KeyValuePair<string, string>(prop.Name, value.ToString()));
                    }
                }
            }

            return dict;
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
