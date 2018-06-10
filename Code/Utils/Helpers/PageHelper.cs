using System;
using System.Text.RegularExpressions;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Common utilities for page handling.
    /// </summary>
    public static class PageHelper
    {
        /// <summary>
        /// Characters that are not allowed in a title.
        /// </summary>
        private static readonly Regex InvalidChars = new Regex(
            @"[^a-zа-я0-9,-]",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Returns the URL-friendly version of the page's title.
        /// </summary>
        public static string EncodeTitle(string title)
        {
            return InvalidChars.Replace(title, "_");
        }

        /// <summary>
        /// Converts the GUID to an equivalent shorter media key.
        /// </summary>
        public static string GetMediaKey(Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray())
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        /// <summary>
        /// Converts the media key to the original GUID.
        /// </summary>
        public static Guid GetMediaId(string key)
        {
            var line = key.Replace('-', '+').Replace('_', '/');
            return new Guid(Convert.FromBase64String(line + "=="));
        }
    }
}
