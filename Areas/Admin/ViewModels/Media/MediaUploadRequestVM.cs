using System.IO;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Data about an uploaded media file.
    /// </summary>
    public class MediaUploadRequestVM
    {
        /// <summary>
        /// Media type.
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// File contents.
        /// </summary>
        public Stream SourceStream { get; set; }
    }
}
