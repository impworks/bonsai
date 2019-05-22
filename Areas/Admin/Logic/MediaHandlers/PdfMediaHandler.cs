using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.NodeServices;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// MediaHandler for creating thumbnails from PDF documents.
    /// </summary>
    public class PdfMediaHandler: IMediaHandler
    {
        public PdfMediaHandler(INodeServices js)
        {
            _js = js;
        }

        private readonly INodeServices _js;

        public bool IsImmediate => true;
        public string[] SupportedMimeTypes => new[] {"application/pdf"};
        public MediaType MediaType => MediaType.Document;

        /// <summary>
        /// Extracts the first page as a thumbnail.
        /// </summary>
        public async Task<Image> ExtractThumbnailAsync(string path, string mime)
        {
            var result = await _js.InvokeAsync<string>("./External/pdf/pdf-thumbnail.js", path);

            if(string.IsNullOrEmpty(result))
                throw new Exception("Failed to render PDF to image: output is empty.");

            var bytes = Convert.FromBase64String(result);
            using(var ms = new MemoryStream(bytes, false))
                return Image.FromStream(ms);
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
