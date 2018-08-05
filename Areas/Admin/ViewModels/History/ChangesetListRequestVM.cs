using System;
using Bonsai.Areas.Admin.ViewModels.Common;

namespace Bonsai.Areas.Admin.ViewModels.History
{
    /// <summary>
    /// Request for changesets.
    /// </summary>
    public class ChangesetListRequestVM: ListRequestVM
    {
        /// <summary>
        /// Filter by entity.
        /// </summary>
        public Guid? EntityId { get; set; }
    }
}
