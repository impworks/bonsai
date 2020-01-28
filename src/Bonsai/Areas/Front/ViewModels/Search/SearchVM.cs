using System.Collections.Generic;

namespace Bonsai.Areas.Front.ViewModels.Search
{
    /// <summary>
    /// The results of a search.
    /// </summary>
    public class SearchVM
    {
        /// <summary>
        /// Search query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Found pages.
        /// </summary>
        public IReadOnlyList<SearchResultVM> Results { get; set; }
    }
}
