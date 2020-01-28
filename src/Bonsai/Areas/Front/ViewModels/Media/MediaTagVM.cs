using System;
using System.Drawing;
using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Front.ViewModels.Media
{
    /// <summary>
    /// A single tag on the photo.
    /// </summary>
    public class MediaTagVM
    {
        /// <summary>
        /// Unique ID of the tag (for cross-highlighting).
        /// </summary>
        public Guid TagId { get; set; }

        /// <summary>
        /// Tagged entity.
        /// </summary>
        public PageTitleVM Page { get; set; }

        /// <summary>
        /// Tag section in photo coordinates (or null for other media types).
        /// </summary>
        public RectangleF? Rect { get; set; }
    }
}
