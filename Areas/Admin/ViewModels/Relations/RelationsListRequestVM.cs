using System;
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
        /// Filter by entity.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Allowed types of relations.
        /// </summary>
        public RelationType[] Types { get; set; }

        /// <summary>
        /// Checks if the request has no filter applied.
        /// </summary>
        public override bool IsEmpty()
        {
            return base.IsEmpty()
                   && EntityId == null
                   && (Types == null || Types.Length == 0);
        }
    }
}
