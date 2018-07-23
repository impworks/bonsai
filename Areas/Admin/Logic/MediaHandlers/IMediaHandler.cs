using System.Threading.Tasks;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// Common interface for handling an uploaded media file.
    /// </summary>
    public interface IMediaHandler
    {
        /// <summary>
        /// List of mime types this handler accepts.
        /// </summary>
        string[] SupportedMimeTypes { get; }

        /// <summary>
        /// Media file classification.
        /// </summary>
        MediaType MediaType { get; }

        /// <summary>
        /// Creates thumbnail files for this media file.
        /// </summary>
        Task CreateThumbnailsAsync(string mime, string path);
    }
}
