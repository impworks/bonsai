using System.Text.RegularExpressions;

namespace Bonsai.Code.Utils
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
    }
}
