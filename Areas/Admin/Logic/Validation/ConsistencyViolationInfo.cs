using System;

namespace Bonsai.Areas.Admin.Logic.Validation
{
    /// <summary>
    /// Information about contradictory facts.
    /// </summary>
    public class ConsistencyViolationInfo
    {
        public ConsistencyViolationInfo(string msg, Guid? pageId, Guid? relationId = null)
        {
            Message = msg;
            PageId = pageId;
            RelationId = relationId;
        }

        /// <summary>
        /// Detailed information about the inconsistency.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Page identifier.
        /// </summary>
        public Guid? PageId { get; }

        /// <summary>
        /// Relation identifier.
        /// </summary>
        public Guid? RelationId { get; }
    }
}
