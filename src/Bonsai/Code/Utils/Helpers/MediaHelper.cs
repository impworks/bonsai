using System;
using Bonsai.Code.Services;
using Bonsai.Data.Models;
using Bonsai.Localization;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for working with media files.
    /// </summary>
    public static class MediaHelper
    {
        /// <summary>
        /// Returns the placeholder title for an unnamed media file.
        /// </summary>
        public static string GetMediaFallbackTitle(MediaType type, DateTimeOffset uploadDate)
        {
            var typePart = type.GetLocaleEnumDescription();
            var datePart = uploadDate.LocalDateTime.ToLocalizedShortDate();
            return string.Format(Texts.Global_MediaFallbackDescription, typePart, datePart);
        }
    }
}
