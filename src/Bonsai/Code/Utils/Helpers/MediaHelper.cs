using System;
using Bonsai.Data.Models;
using Impworks.Utils.Format;

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
            var typePart = type.GetEnumDescription();
            var datePart = uploadDate.LocalDateTime.ToRussianShortDate();
            return $"{typePart} от {datePart}";
        }
    }
}
