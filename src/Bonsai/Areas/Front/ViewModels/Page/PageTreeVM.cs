using System.Collections.Generic;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Details of the page tree.
    /// </summary>
    public class PageTreeVM: PageTitleVM
    {
        /// <summary>
        /// List of tree kinds which are enabled in the settings.
        /// </summary>
        public IReadOnlyList<TreeKind> SupportedKinds { get; set; }

        /// <summary>
        /// Kind of the tree being displayed.
        /// </summary>
        public TreeKind TreeKind { get; set; }
    }
}
