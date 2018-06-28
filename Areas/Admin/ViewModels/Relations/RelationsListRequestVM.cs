using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Relations
{
    /// <summary>
    /// Request for finding relations.
    /// </summary>
    public class RelationsListRequestVM: ListRequestVM
    {
        /// <summary>
        /// Allowed types of relations.
        /// </summary>
        public RelationType[] Types { get; set; }
    }
}
