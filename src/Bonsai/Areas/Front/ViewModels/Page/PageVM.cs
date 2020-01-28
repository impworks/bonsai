using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Details of the page.
    /// </summary>
    public class PageVM<T> where T: PageTitleVM
    {
        /// <summary>
        /// Main page contents.
        /// </summary>
        public T Body { get; set; }

        /// <summary>
        /// Additional info in the sidebar.
        /// </summary>
        public InfoBlockVM InfoBlock { get; set; }
    }
}
