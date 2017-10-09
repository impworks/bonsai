using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// Descriptions of the media's 
    /// </summary>
    public class PageMediaVM
    {
        /// <summary>
        /// The list of media item thumbnails for current page.
        /// </summary>
        public IEnumerable<MediaThumbnailVM> Thumbnails { get; set; }
    }
}
