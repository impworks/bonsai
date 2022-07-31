using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SixLabors.ImageSharp;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// The background service for encoding uploaded media.
    /// </summary>
    public class MediaEncoderService: WorkerServiceBase
    {
        #region Constructor

        public MediaEncoderService(
            WorkerAlarmService alarm,
            IServiceProvider services,
            IWebHostEnvironment env,
            ILogger logger,
            CacheService cache
        )
            : base(services)
        {
            _env = env;
            _logger = logger;
            _cache = cache;

            alarm.OnNewEncoderJob += (s, e) =>
            {
                _isAsleep = false;
            };
        }

        #endregion

        #region Fields

        private readonly IWebHostEnvironment _env;
        private readonly ILogger _logger;
        private readonly CacheService _cache;

        #endregion

        #region Encoder logic

        /// <summary>
        /// Main loop.
        /// </summary>
        protected override async Task<bool> ProcessAsync(IServiceProvider services)
        {
            await services.GetRequiredService<StartupService>().WaitForStartup();

            using var db = services.GetService<AppDbContext>();
            var job = await db.MediaJobs
                              .Include(x => x.Media)
                              .OrderBy(x => x.Media.UploadDate)
                              .FirstOrDefaultAsync();

            if (job == null)
                return true;

            try
            {
                if (job.Media.Type == MediaType.Video)
                    await EncodeVideoAsync(job);
                else
                    throw new ArgumentException($"Unsupported media type: {job.Media.Type}");
                
                _cache.Remove<MediaVM>(job.Media.Key);
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException))
                    _logger.Error(ex, "Failed to convert media.");
            }

            db.MediaJobs.Remove(job);
            await db.SaveChangesAsync();

            return false;
        }

        #endregion

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
        private async Task EncodeVideoAsync(MediaEncodingJob job)
        {
            var path = job.Media.FilePath.Replace("~/", "");
            var inputPath = Path.Combine(_env.WebRootPath, path);
            var encodedPath = Path.ChangeExtension(inputPath, ".mp4");

            await EncodeVideoToMp4Async(inputPath, encodedPath);
            await CreateVideoThumbnailAsync(encodedPath);

            job.Media.IsProcessed = true;
            job.Media.FilePath = Path.ChangeExtension(job.Media.FilePath, ".mp4");
        }

        /// <summary>
        /// Extracts a screenshot from the center of the video.
        /// </summary>
        private async Task CreateVideoThumbnailAsync(string path)
        {
            _logger.Information($"Thumbnail extraction started for video file: {path}");

            var durationRaw = await ProcessHelper.GetOutputAsync(GetFFPath("ffprobe"), $@"-i ""{path}"" -show_entries format=duration -v quiet -of csv=""p=0""");
            var duration = durationRaw.Replace(",", ".").TryParse<double?>();

            if (duration == null)
                _logger.Error($"Failed to get media duration: '{durationRaw}' is not a valid number.");

            var position = (int) ((duration ?? 0) / 2);

            var screenPath = Path.ChangeExtension(path, ".jpg");
            await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), $@"-i ""{path}"" -y -vframes 1 -ss {position} ""{screenPath}""");

            _logger.Information("Thumbnail extraction completed.");

            using var img = await Image.LoadAsync(screenPath);
            await MediaHandlerHelper.CreateThumbnailsAsync(screenPath, img);

            File.Delete(screenPath);
        }

        /// <summary>
        /// Encodes the video to MP4.
        /// </summary>
        private async Task EncodeVideoToMp4Async(string inputPath, string outputPath)
        {
            if (inputPath == outputPath)
                return;

            // todo: check actual format?

            _logger.Information($"Conversion started for video file: {inputPath}");

            await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), $@"-i ""{inputPath}"" -f mp4 -vcodec libx264 -preset fast -profile:v main -acodec aac -hide_banner -movflags +faststart -threads 0 ""{outputPath}""");
            File.Delete(inputPath);

            _logger.Information("Video conversion completed.");
        }

        #endregion
    }
}
