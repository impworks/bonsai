using System;
using Bonsai.Areas.Front.ViewModels.Media;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Additional information about a media file.
    /// </summary>
    public class MediaThumbnailExtendedVM: MediaThumbnailVM
    {
        /// <summary>
        /// Date of the media file's upload.
        /// </summary>
        public DateTimeOffset UploadDate { get; set; }
    }
}
