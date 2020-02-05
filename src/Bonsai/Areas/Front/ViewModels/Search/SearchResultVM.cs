using Bonsai.Areas.Front.ViewModels.Page;

namespace Bonsai.Areas.Front.ViewModels.Search
{
    /// <summary>
    /// Information about a page found in the search.
    /// </summary>
    public class SearchResultVM: PageTitleExtendedVM
    {
        /// <summary>
        /// Page title with matched elements highlighted.
        /// </summary>
        public string HighlightedTitle { get; set; }

        /// <summary>
        /// A portion of the page's description that matches the query.
        /// </summary>
        public string DescriptionExcerpt { get; set; }
    }
}
