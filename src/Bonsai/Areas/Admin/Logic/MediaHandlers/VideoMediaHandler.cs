using System.IO;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers;

/// <summary>
/// MediaHandler for creating thumbnails for photos.
/// </summary>
public class VideoMediaHandler: IMediaHandler
{
    public VideoMediaHandler(IWebHostEnvironment env)
    {
        _env = env;
    }

    private readonly IWebHostEnvironment _env;

    public bool IsImmediate => false;
    public MediaType MediaType => MediaType.Video;
    public string[] SupportedMimeTypes =>
    [
        "video/mp4",
        "video/x-flv",
        "video/ogg",
        "video/quicktime",
        "video/x-msvideo",
        "video/x-matroska"
    ];

    /// <summary>
    /// Returns the image for thumbnail creation.
    /// </summary>
    public async Task<Image> ExtractThumbnailAsync(string path, string mime)
    {
        var thumbPath = Path.Combine(_env.WebRootPath, "assets", "img", "video-thumb.png");
        return await Image.LoadAsync(thumbPath);
    }

    /// <summary>
    /// Extracts additional data from the media.
    /// </summary>
    public Task<MediaMetadata> ExtractMetadataAsync(string path, string mime)
    {
        return Task.FromResult<MediaMetadata>(null);
    }
}