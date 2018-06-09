using System;
using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Information about the page with a tiny preview image, if any.
    /// </summary>
    public class PageTitleExtendedVM: PageTitleVM
    {
        /// <summary>
        /// Page's main image.
        /// </summary>
        public string MainPhotoPath { get; set; }

        /// <summary>
        /// Last update.
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
