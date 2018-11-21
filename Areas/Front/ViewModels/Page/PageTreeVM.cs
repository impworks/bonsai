using Bonsai.Areas.Front.ViewModels.Tree;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Elements of the page's tree.
    /// </summary>
    public class PageTreeVM: PageTitleVM
    {
        /// <summary>
        /// Tree data.
        /// </summary>
        public TreeVM Tree { get; set; }
    }
}
