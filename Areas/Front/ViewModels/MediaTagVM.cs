using System.Drawing;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// A single tag on the photo.
    /// </summary>
    public class MediaTagVM
    {
        /// <summary>
        /// Tagged entity.
        /// </summary>
        public PageLinkVM Page { get; set; }

        /// <summary>
        /// Tag section in photo coordinates (or null for other media types).
        /// </summary>
        public Rectangle? Rect { get; set; }
    }
}
