using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// The list of facts related to a particular page.
    /// </summary>
    public class PageFactsVM: PageTitleVM
    {
        /// <summary>
        /// Groups of facts related to this page.
        /// </summary>
        public IEnumerable<FactGroupVM> FactGroups { get; set; }
    }
}
