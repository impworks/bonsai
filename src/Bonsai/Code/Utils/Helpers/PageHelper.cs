using System;
using System.Text.RegularExpressions;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;

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
            @"[^a-zа-яё0-9,-]",
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

        /// <summary>
        /// Returns the server-local path for the page's image.
        /// </summary>
        public static string GetPageImageUrl(PageType? type, string image, string fallback = null, MediaSize? size = null)
        {
            var imageSized = size != null && !string.IsNullOrEmpty(image)
                ? MediaPresenterService.GetSizedMediaPath(image, size.Value)
                : image;

            var typeStr = (type ?? PageType.Person).ToString().ToLower();
            return StringHelper.Coalesce(imageSized, fallback, $"~/assets/img/unknown-{typeStr}.svg");
        }
    }
}
