using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for rendering data.
    /// </summary>
    public class ViewHelper
    {
        /// <summary>
        /// Returns the select list with current element selected.
        /// </summary>
        public static IReadOnlyList<SelectListItem> GetEnumSelectList<T>(T? value, bool addEmpty = true, T[] except = null)
            where T : struct, IConvertible, IComparable
        {
            var list = new List<SelectListItem>();
            if(value == null && addEmpty)
                list.Add(new SelectListItem { Text = "Не выбрано", Selected = true });

            foreach(var entry in EnumHelper.GetEnumDescriptions<T>())
            {
                if (except?.Contains(entry.Key) == true)
                    continue;

                list.Add(new SelectListItem
                {
                    Text = entry.Value,
                    Value = Convert.ToInt32(entry.Key).ToString(),
                    Selected = entry.Key.CompareTo(value) == 0
                });
            }

            return list;
        }

        /// <summary>
        /// Returns the select list with current element selected.
        /// </summary>
        public static IReadOnlyList<SelectListItem> GetEnumSelectList<T>(T value, T[] except = null)
            where T : struct, IConvertible, IComparable
        {
            return GetEnumSelectList(value, false, except);
        }

        /// <summary>
        /// Returns the Gravatar URL for an email address.
        /// </summary>
        public static string GetGravatarUrl(string email)
        {
            var cleanEmail = (email ?? "").ToLowerInvariant().Trim();
            return "https://www.gravatar.com/avatar/" + Md5(cleanEmail);
        }

        /// <summary>
        /// Renders a bullet list with specified items.
        /// </summary>
        public static string RenderBulletList(IHtmlHelper html, IEnumerable<string> items)
        {
            var sb = new StringBuilder("<ul>");

            foreach (var item in items)
            {
                sb.Append("<li>");
                sb.Append(html.Encode(item));
                sb.Append("</li>");
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        /// <summary>
        /// Renders the media thumbnail in a wrapper.
        /// </summary>
        public static string RenderMediaThumbnail(string url)
        {
            return $@"
                <div class=""media-thumb-wrapper"">
                    <div class=""media-thumb-square"" style=""background-image: url('{url}')""></div>
                </div>
            ";
        }

        #region Helpers

        /// <summary>
        /// Returns the MD5 hash of a string.
        /// </summary>
        private static string Md5(string str)
        {
            using(var md5 = MD5.Create())
            {
                var input = Encoding.UTF8.GetBytes(str);
                var output = md5.ComputeHash(input);

                var sb = new StringBuilder();
                foreach(var b in output)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }

        #endregion
    }
}
