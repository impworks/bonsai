using System;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Request for changesets.
    /// </summary>
    public class ChangesetsListRequestVM: ListRequestVM
    {
        /// <summary>
        /// Filter by entity.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Found entity types.
        /// </summary>
        public ChangesetEntityType[] EntityTypes { get; set; }
    }
}
