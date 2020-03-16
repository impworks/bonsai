using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Dashboard information.
    /// </summary>
    public class DashboardVM
    {
        /// <summary>
        /// Last edits made by users.
        /// </summary>
        public IReadOnlyList<ChangesetGroupVM> Changesets { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int PagesCount { get; set; }

        /// <summary>
        /// Number of pages with a low completion score that need more info.
        /// </summary>
        public int PagesToImproveCount { get; set; }

        /// <summary>
        /// Total number of media.
        /// </summary>
        public int MediaCount { get; set; }

        /// <summary>
        /// Number of photos without tags.
        /// </summary>
        public int MediaToTagCount { get; set; }

        /// <summary>
        /// Total number of relations.
        /// </summary>
        public int RelationsCount { get; set; }
    }
}
