using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers;

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
    public string[] SupportedMimeTypes =>
    [
        "image/jpeg",
        "image/png"
    ];

    /// <summary>
    /// Returns the image for thumbnail creation.
    /// </summary>
    public async Task<Image> ExtractThumbnailAsync(string path, string mime)
    {
        return await Image.LoadAsync(path);
    }

    /// <summary>
    /// Extracts additional data from the media.
    /// </summary>
    public async Task<MediaMetadata> ExtractMetadataAsync(string path, string mime)
    {
        try
        {
            using var image = await Image.LoadAsync(path);
            var exif = image.Metadata.ExifProfile;
            var dateRaw = exif.Values?.FirstOrDefault(x => x.Tag == ExifTag.DateTimeOriginal)?.GetValue();

            return new MediaMetadata
            {
                Date = ParseDate(dateRaw?.ToString())
            };
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