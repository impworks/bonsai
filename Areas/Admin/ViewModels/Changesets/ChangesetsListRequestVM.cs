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
        public ChangesetsListRequestVM()
        {
            OrderDescending = true;
        }

        /// <summary>
        /// Filter by entity.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Filter by author.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Found entity types.
        /// </summary>
        public ChangesetEntityType[] EntityTypes { get; set; }

        /// <summary>
        /// Checks if the request has no filter applied.
        /// </summary>
        public override bool IsEmpty()
        {
            return base.IsEmpty()
                && EntityId == null
                && string.IsNullOrEmpty(UserId)
                && (EntityTypes == null || EntityTypes.Length == 0);
        }
    }
}
