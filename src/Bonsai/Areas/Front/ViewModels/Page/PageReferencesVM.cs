using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Page with references to current page.
    /// </summary>
    public class PageReferencesVM : PageTitleVM
    {
        /// <summary>
        /// List of other pages referencing the current one.
        /// </summary>
        public IReadOnlyList<PageTitleVM> References { get; set; }
    }
}