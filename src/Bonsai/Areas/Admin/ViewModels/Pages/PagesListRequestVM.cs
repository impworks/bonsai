using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// The request for filtering pages.
    /// </summary>
    public class PagesListRequestVM: ListRequestVM
    {
        /// <summary>
        /// List of selected page types.
        /// </summary>
        public PageType[] Types { get; set; }
        
        /// <summary>
        /// Checks if the request has no filter applied.
        /// </summary>
        public override bool IsEmpty()
        {
            return base.IsEmpty()
                   && (Types == null || Types.Length == 0);
        }
    }
}
