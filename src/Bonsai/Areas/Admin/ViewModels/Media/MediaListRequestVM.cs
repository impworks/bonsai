using System;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// The request for filtering media files.
    /// </summary>
    public class MediaListRequestVM: ListRequestVM
    {
        /// <summary>
        /// Related page.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Related media types.
        /// </summary>
        public MediaType[] Types { get; set; }
        
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
