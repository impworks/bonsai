using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bonsai.Data.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// MediaHandler for creating thumbnails for photos.
    /// </summary>
    public class PhotoMediaHandler: IMediaHandler
    {
        public PhotoMediaHandler(ILogger logger)
        {
            _logger = logger;
        }

        private readonly ILogger _logger;

        private static string[] _supportedMimeTypes =
        {
            "image/jpeg",
            "image/png"
        };

        /// <summary>
        /// Flag indicating that the media does not need any encoding.
        /// </summary>
        public bool IsImmediate => true;

        /// <summary>
        /// Supported file types.
        /// </summary>
        public string[] SupportedMimeTypes => _supportedMimeTypes;

        /// <summary>
        /// Resulting media type.
        /// </summary>
        public MediaType MediaType => MediaType.Photo;

        /// <summary>
        /// Returns the image for thumbnail creation.
        /// </summary>
        public Image ExtractThumbnail(string path, string mime)
        {
            return Image.FromFile(path);
        }

        /// <summary>
        /// Extracts additional data from the media.
        /// </summary>
        public MediaMetadata ExtractMetadata(string path, string mime)
        {
            try
            {
                var dirs = ImageMetadataReader.ReadMetadata(path);
                var dateStr = dirs.OfType<ExifSubIfdDirectory>()
                               .FirstOrDefault()
                               ?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

                return new MediaMetadata
                {
                    Date = ParseDate(dateStr)
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get photo metadata!");
                return null;
            }
        }

        /// <summary>
        /// Parses the date from an EXIF raw value.
        /// </summary>
        private DateTime? ParseDate(string dateRaw)
        {
            if (string.IsNullOrEmpty(dateRaw))
                return null;

            var dateFixed = Regex.Replace(dateRaw, @"^(?<year>\d{4}):(?<month>\d{2}):(?<day>\d{2})", "${year}/${month}/${day}");

            if (DateTime.TryParse(dateFixed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            return null;
        }
    }
}
