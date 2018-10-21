using System.IO;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Data about an uploaded media file.
    /// </summary>
    public class MediaUploadRequestVM
    {
        /// <summary>
        /// Name of the original file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// MIME type of the file.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// File contents.
        /// </summary>
        public Stream Data { get; set; }

        /// <summary>
        /// Optional title for the page.
        /// </summary>
        public string Title { get; set; }
    }
}
