using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    public class PageVMBase: PageTitleVM
    {
        /// <summary>
        /// Block of facts and relations info.
        /// </summary>
        public InfoBlockVM InfoBlock { get; set; }
    }
}
