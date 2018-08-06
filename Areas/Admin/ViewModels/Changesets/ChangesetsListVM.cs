using Bonsai.Areas.Admin.ViewModels.Common;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// List of found changesets.
    /// </summary>
    public class ChangesetsListVM: ListResultVM<ChangesetsListRequestVM, ChangesetTitleVM>
    {
    }
}
