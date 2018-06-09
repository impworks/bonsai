using Bonsai.Areas.Admin.ViewModels.Dashboard;

namespace Bonsai.Areas.Front.ViewModels.Search
{
    /// <summary>
    /// Information about a page found in the search.
    /// </summary>
    public class SearchResultVM: PageTitleExtendedVM
    {
        /// <summary>
        /// A portion of the page's description that matches the query.
        /// </summary>
        public string DescriptionExcerpt { get; set; }
    }
}
