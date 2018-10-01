using Bonsai.Areas.Admin.ViewModels.Common;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Found relations.
    /// </summary>
    public class RelationsListVM: ListResultVM<RelationsListRequestVM, RelationTitleVM>
    {
        /// <summary>
        /// Title of the page to filter by.
        /// </summary>
        public string EntityTitle { get; set; }
    }
}
