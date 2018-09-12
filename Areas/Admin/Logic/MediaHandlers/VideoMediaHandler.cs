using System.Drawing;
using System.IO;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Hosting;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// MediaHandler for creating thumbnails for photos.
    /// </summary>
    public class VideoMediaHandler: IMediaHandler
    {
        public VideoMediaHandler(IHostingEnvironment env)
        {
            _env = env;
        }

        private readonly IHostingEnvironment _env;

        private static string[] _supportedMimeTypes =
        {
            "video/mp4",
            "video/x-flv",
            "video/quicktime",
            "video/x-msvideo",
        };

        /// <summary>
        /// Flag indicating that the media does not need any encoding.
        /// </summary>
        public bool IsImmediate => false;

        /// <summary>
        /// Supported file types.
        /// </summary>
        public string[] SupportedMimeTypes => _supportedMimeTypes;

        /// <summary>
        /// Resulting media type.
        /// </summary>
        public MediaType MediaType => MediaType.Video;

        /// <summary>
        /// Returns the image for thumnail creation.
        /// </summary>
        public Image ExtractThumbnail(string path, string mime)
        {
            return Image.FromFile(Path.Combine(_env.WebRootPath, "assets", "img", "video-thumb.png"));
        }
    }
}
