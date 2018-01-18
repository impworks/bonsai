using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Home
{
    /// <summary>
    /// Model to display on the main page.
    /// </summary>
    public class HomeVM
    {
        /// <summary>
        /// List of last updated pages.
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> LastUpdatedPages { get; set; }

        /// <summary>
        /// List of last uploaded files.
        /// </summary>
        public IReadOnlyList<MediaThumbnailExtendedVM> LastUploadedMedia { get; set; }
    }
}
