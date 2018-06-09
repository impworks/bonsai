using System.Collections.Generic;
using Bonsai.Areas.Admin.ViewModels.User;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Dashboard information.
    /// </summary>
    public class DashboardVM
    {
        /// <summary>
        /// Newly updated pages.
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> UpdatedPages { get; set; }

        /// <summary>
        /// Newly uploaded media files.
        /// </summary>
        public IReadOnlyList<MediaThumbnailExtendedVM> UploadedMedia { get; set; }

        /// <summary>
        /// Newly registered users.
        /// </summary>
        public IReadOnlyList<UserTitleVM> NewUsers { get; set; }
    }
}
