using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Media;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Descriptions of the media's 
    /// </summary>
    public class PageMediaVM: PageTitleVM
    {
        /// <summary>
        /// The list of media item thumbnails for current page.
        /// </summary>
        public IEnumerable<MediaThumbnailVM> Media { get; set; }
    }
}
