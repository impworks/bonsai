using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// The description section of a page.
    /// </summary>
    public class PageDescriptionVM: PageTitleVM
    {
        /// <summary>
        /// Main description (in HTML format).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Block of facts and relations info.
        /// </summary>
        public InfoBlockVM InfoBlock { get; set; }
    }
}
