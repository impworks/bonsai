using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// The background service for encoding uploaded media.
    /// </summary>
    public class MediaEncoderService: IHostedService
    {
        #region Constructor

        public MediaEncoderService(IServiceProvider services, IHostingEnvironment env)
        {
            _services = services;
            _env = env;
            _cancellationSource = new CancellationTokenSource();
        }

        #endregion

        #region Fields

        private readonly IServiceProvider _services;
        private readonly IHostingEnvironment _env;
        private readonly CancellationTokenSource _cancellationSource;

        private Thread _thread;

        private CancellationToken Token => _cancellationSource.Token;

        #endregion

        #region IHostedService implementation

        /// <summary>
        /// Starts the background encoder service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _thread = new Thread(MainLoop) {IsBackground = true};
            _thread.Start();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationSource.Cancel();

            return Task.CompletedTask;
        }

        #endregion

        #region Encoder logic

        /// <summary>
        /// Sync wrapper for the main loop.
        /// </summary>
        private void MainLoop()
        {
            MainLoopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Main loop.
        /// </summary>
        private async Task MainLoopAsync()
        {
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<AppDbContext>();

                while (true)
                {
                    try
                    {
                        var job = await db.MediaJobs
                                          .Include(x => x.Media)
                                          .OrderBy(x => x.Media.UploadDate)
                                          .FirstOrDefaultAsync();

                        if (job != null)
                        {
                            if (job.Media.Type == MediaType.Video)
                                await EncodeVideoAsync(job);
                            else
                                throw new ArgumentException("Unsupported media type: ");

                            db.MediaJobs.Remove(job);
                            await db.SaveChangesAsync();
                        }

                        await Task.Delay(TimeSpan.FromSeconds(30), Token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.Demystify(), "An error occured while encoding media.");
                    }
                }
            }
        }

        /// <summary>
        /// Returns the path to a FFMpeg executable.
        /// </summary>
        private string GetFFPath(string executable)
        {
            return Path.Combine(_env.ContentRootPath, "External", "ffmpeg", executable);
        }

        #endregion

        #region Video encoding

        /// <summary>
        /// Encodes a video file.
        /// </summary>
        private async Task EncodeVideoAsync(MediaEncodingJob job)
        {
            var path = job.Media.FilePath.Replace("~/", "");
            var inputPath = Path.Combine(_env.WebRootPath, path);
            var encodedPath = Path.ChangeExtension(inputPath, ".mp4");

            await CreateVideoThumbnailAsync(inputPath);
            await EncodeVideoToMp4Async(inputPath, encodedPath);

            job.Media.IsProcessed = true;
            job.Media.FilePath = Path.ChangeExtension(job.Media.FilePath, ".mp4");
        }

        /// <summary>
        /// Extracts a screenshot from the center of the video.
        /// </summary>
        private async Task CreateVideoThumbnailAsync(string path)
        {
            var durationRaw = await ProcessHelper.GetOutputAsync(GetFFPath("ffprobe"), $@"-i ""{path}"" -show_entries format=duration -v quiet -of csv=""p=0""");
            var duration = durationRaw.Parse<double>();
            var middle = (int) (duration / 2);

            var screenPath = Path.ChangeExtension(path, ".jpg");
            await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), $@"-i ""{path}"" -y -f mjpeg -vframes 1 -ss {middle} ""{screenPath}""");

            using(var img = Image.FromFile(screenPath))
                MediaHandlerHelper.CreateThumbnails(screenPath, img);

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

            await ProcessHelper.InvokeAsync(GetFFPath("ffmpeg"), $@"-i ""{inputPath}"" -f mp4 -vcodec libx264 -preset veryslow -profile:v main -acodec aac -hide_banner -movflags +faststart -threads 0 ""{outputPath}""");
            File.Delete(inputPath);
        }

        #endregion
    }
}
