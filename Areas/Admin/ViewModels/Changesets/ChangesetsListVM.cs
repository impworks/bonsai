using Bonsai.Areas.Admin.ViewModels.Common;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// List of found changesets.
    /// </summary>
    public class ChangesetsListVM: ListResultVM<ChangesetsListRequestVM, ChangesetTitleVM>
    {
        /// <summary>
        /// Title of the filter-by entity.
        /// </summary>
        public string EntityTitle { get; set; }

        /// <summary>
        /// Title of the filter-by user.
        /// </summary>
        public string UserTitle { get; set; }
    }
}
