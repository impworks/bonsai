using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// Details about a media item's thumbnail.
    /// </summary>
    public class MediaThumbnailVM
    {
        /// <summary>
        /// URL of the media thumbnail's file.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Full URL of the media's display page.
        /// </summary>
        public string DisplayUrl { get; set; }

        /// <summary>
        /// Type of the media. 
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Year of the media's origin.
        /// </summary>
        public string Year { get; set; }
    }
}
