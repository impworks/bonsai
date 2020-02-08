using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.ViewModels.Media
{
    /// <summary>
    /// Details about a media item's thumbnail.
    /// </summary>
    public class MediaThumbnailVM
    {
        /// <summary>
        /// Full URL of the media's display page.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// URL of the media's thumbnail.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Type of the media. 
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Date of the media's origin.
        /// </summary>
        public FuzzyDate? Date { get; set; }
    }
}
