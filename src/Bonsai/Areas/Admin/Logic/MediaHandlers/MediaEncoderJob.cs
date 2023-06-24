using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using SixLabors.ImageSharp;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers;

public class MediaEncoderJob: JobBase<Guid>
{
    public MediaEncoderJob(AppDbContext db, IWebHostEnvironment env, ILogger logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }
    
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger _logger;

    protected override async Task ExecuteAsync(Guid mediaId, CancellationToken token)
    {
        var media = await _db.Media.GetAsync(x => x.Id == mediaId, $"Media {mediaId} not found.");
        await ProcessVideoAsync(media, token);
        await _db.SaveChangesAsync(CancellationToken.None);
    }
    
    #region Video encoding

    /// <summary>
    /// Returns the path to a FFMpeg executable.
    /// </summary>
    private string GetFFPath(string executable)
    {
        return Path.Combine(_env.ContentRootPath, "External", "ffmpeg", executable);
    }

    /// <summary>
    /// Encodes a video file.
    /// </summary>
    private async Task ProcessVideoAsync(Media media, CancellationToken token)
    {
        var path = media.FilePath.Replace("~/", "");
        var inputPath = Path.Combine(_env.WebRootPath, path);
        var encodedPath = Path.ChangeExtension(inputPath, ".mp4");

        await EncodeVideoToMp4Async(inputPath, encodedPath, token);
        await CreateVideoThumbnailAsync(encodedPath, token);

        media.IsProcessed = true;
        media.FilePath = Path.ChangeExtension(media.FilePath, ".mp4");
    }

    /// <summary>
    /// Extracts a screenshot from the center of the video.
    /// </summary>
    private async Task CreateVideoThumbnailAsync(string path, CancellationToken token)
    {
        _logger.Information($"Thumbnail extraction started for video file: {path}");

        var durationRaw = await ProcessHelper.GetOutputAsync(GetFFPath("ffprobe"), $@"-i ""{path}"" -show_entries format=duration -v quiet -of csv=""p=0""", token);
        var duration = durationRaw.Replace(",", ".").TryParse<double?>();

        if (duration == null)
            _logger.Error($"Failed to get media duration: '{durationRaw}' is not a valid number.");

        var position = (int) ((duration ?? 0) / 2);

        var screenPath = Path.ChangeExtension(path, ".jpg");
        await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), $@"-i ""{path}"" -y -vframes 1 -ss {position} ""{screenPath}""", token);

        _logger.Information("Thumbnail extraction completed.");

        using var img = await Image.LoadAsync(screenPath, token);
        await MediaHandlerHelper.CreateThumbnailsAsync(screenPath, img, token);

        File.Delete(screenPath);
    }

    /// <summary>
    /// Encodes the video to MP4.
    /// </summary>
    private async Task EncodeVideoToMp4Async(string inputPath, string outputPath, CancellationToken token)
    {
        if (inputPath == outputPath)
            return;

        // todo: check actual format?

        _logger.Information($"Conversion started for video file: {inputPath}");

        var args = $@"-i ""{inputPath}"" -f mp4 -vcodec libx264 -preset fast -profile:v main -acodec aac -hide_banner -movflags +faststart -threads 0 ""{outputPath}""";
        await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), args, token);
        File.Delete(inputPath);

        _logger.Information("Video conversion completed.");
    }

    #endregion
}