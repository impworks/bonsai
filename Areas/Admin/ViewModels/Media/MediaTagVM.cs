using System;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Information about a single tagged entity on the photo.
    /// </summary>
    public class MediaTagVM
    {
        /// <summary>
        /// ID of the tagged entity (if specified).
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// Title of the tagged entity (if no page is specified).
        /// </summary>
        public string ObjectTitle { get; set; }

        /// <summary>
        /// Semicolon-separated coordinates of the tag.
        /// </summary>
        public string Coordinates { get; set; }
    }
}
