using System.Collections.Generic;
using Bonsai.Areas.Admin.ViewModels.Dashboard;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Found pages.
    /// </summary>
    public class PagesListVM
    {
        /// <summary>
        /// Current search query.
        /// </summary>
        public PagesListRequestVM Request { get; set; }

        /// <summary>
        /// List of pages.
        /// </summary>
        public IReadOnlyList<PageTitleExtendedVM> Items { get; set; }

        /// <summary>
        /// Number of pages of data.
        /// </summary>
        public int PageCount { get; set; }
    }
}
