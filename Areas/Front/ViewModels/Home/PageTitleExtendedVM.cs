using System;
using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Front.ViewModels.Home
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
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
