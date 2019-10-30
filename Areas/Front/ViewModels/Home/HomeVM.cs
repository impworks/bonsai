using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Front.ViewModels.Home
{
    /// <summary>
    /// VM for the home page.
    /// </summary>
    public class HomeVM
    {
        /// <summary>
        /// List of the pages with latest updates.
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> LastUpdatedPages { get; set; }

        /// <summary>
        /// List of the latest uploaded media.
        /// </summary>
        public IReadOnlyList<MediaThumbnailVM> LastUploadedMedia { get; set; }
    }
}
