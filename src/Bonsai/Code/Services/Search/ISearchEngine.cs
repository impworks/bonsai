using System.Collections.Generic;
using System.Threading.Tasks;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services.Search
{
    /// <summary>
    /// Common interface for fulltext search engines.
    /// </summary>
    public interface ISearchEngine
    {
        Task InitializeAsync();

        Task AddPageAsync(Page page);
        Task RemovePageAsync(Page page);
        Task ClearDataAsync();

        Task<IReadOnlyList<PageDocumentSearchResult>> SearchAsync(string query, int page = 0);
        Task<IReadOnlyList<PageDocumentSearchResult>> SuggestAsync(string query, IReadOnlyList<PageType> pageTypes = null, int? maxCount = null);
    }
}
