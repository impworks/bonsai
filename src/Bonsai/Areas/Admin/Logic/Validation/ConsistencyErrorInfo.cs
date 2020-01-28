using System;

namespace Bonsai.Areas.Admin.Logic.Validation
{
    /// <summary>
    /// Information about contradictory facts.
    /// </summary>
    public class ConsistencyErrorInfo
    {
        public ConsistencyErrorInfo(string msg, params Guid[] pageIds)
        {
            Message = msg;
            PageIds = pageIds;
        }

        /// <summary>
        /// Detailed information about the inconsistency.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Related pages.
        /// </summary>
        public Guid[] PageIds { get; }
    }
}
