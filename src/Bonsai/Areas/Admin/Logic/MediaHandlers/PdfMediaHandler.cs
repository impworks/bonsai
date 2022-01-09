using System.IO;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// MediaHandler for creating thumbnails from PDF documents.
    /// </summary>
    public class PdfMediaHandler: IMediaHandler
    {
        public PdfMediaHandler(IWebHostEnvironment env)
        {
            _env = env;
        }

        private readonly IWebHostEnvironment _env;

        public bool IsImmediate => true;
        public string[] SupportedMimeTypes => new[] {"application/pdf"};
        public MediaType MediaType => MediaType.Document;

        /// <summary>
        /// Extracts the first page as a thumbnail.
        /// </summary>
        public async Task<Image> ExtractThumbnailAsync(string path, string mime)
        {
            var thumbPath = Path.Combine(_env.WebRootPath, "assets", "img", "pdf-thumb.png");
            return await Image.LoadAsync(thumbPath);
        }

        /// <summary>
        /// No metadata extraction is supported.
        /// </summary>
        public Task<MediaMetadata> ExtractMetadataAsync(string path, string mime)
        {
            return Task.FromResult<MediaMetadata>(null);
        }
    }
}