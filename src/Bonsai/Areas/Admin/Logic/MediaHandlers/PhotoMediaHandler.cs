using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public bool IsImmediate => true;
        public MediaType MediaType => MediaType.Photo;
        public string[] SupportedMimeTypes => new []
        {
            "image/jpeg",
            "image/png"
        };

        /// <summary>
        /// Returns the image for thumbnail creation.
        /// </summary>
        public Task<Image> ExtractThumbnailAsync(string path, string mime)
        {
            return Task.FromResult(Image.FromFile(path));
        }

        /// <summary>
        /// Extracts additional data from the media.
        /// </summary>
        public Task<MediaMetadata> ExtractMetadataAsync(string path, string mime)
        {
            try
            {
                var dirs = ImageMetadataReader.ReadMetadata(path);
                var dateStr = dirs.OfType<ExifSubIfdDirectory>()
                                  .FirstOrDefault()
                                  ?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

                return Task.FromResult(new MediaMetadata
                {
                    Date = ParseDate(dateStr)
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get photo metadata!");
                return null;
            }
        }

        #region Private helpers

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

        #endregion
    }
}
