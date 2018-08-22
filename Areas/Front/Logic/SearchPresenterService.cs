using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Search;
using Bonsai.Code.Services.Elastic;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The presenter for search results.
    /// </summary>
    public class SearchPresenterService
    {
        public SearchPresenterService(AppDbContext db, ElasticService elastic)
        {
            _db = db;
            _elastic = elastic;
        }

        private readonly AppDbContext _db;
        private readonly ElasticService _elastic;

        private const int MIN_QUERY_LENGTH = 3;

        /// <summary>
        /// Find pages matching the query.
        /// </summary>
        public async Task<IReadOnlyList<SearchResultVM>> SearchAsync(string query, int page = 0)
        {
            var q = (query ?? "").Trim();
            if(q.Length < MIN_QUERY_LENGTH)
                return new SearchResultVM[0];

            var matches = await _elastic.SearchAsync(q, page);
            var ids = matches.Select(x => x.Id);

            var details = await _db.Pages
                                   .Where(x => ids.Contains(x.Id) && x.IsDeleted == false)
                                   .Select(x => new { x.Id, x.MainPhoto.FilePath, x.LastUpdateDate, PageType = x.Type })
                                   .ToDictionaryAsync(x => x.Id, x => x);

            var results = matches.Select(x => new SearchResultVM
            {
                Key = x.Key,
                Title = x.HighlightedTitle,
                Type = details[x.Id].PageType,
                DescriptionExcerpt = x.HighlightedDescription,
                MainPhotoPath = details[x.Id].FilePath,
                LastUpdateDate = details[x.Id].LastUpdateDate,
            });

            return results.ToList();
        }

        /// <summary>
        /// Shows autocomplete suggestions for the search box.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleVM>> SearchAutocompleteAsync(string query)
        {
            var q = (query ?? "").Trim();
            if(q.Length < MIN_QUERY_LENGTH)
                return new PageTitleVM[0];

            var results = await _elastic.SearchAutocompleteAsync(q);

            return results.Select(x => new PageTitleVM {Title = x.HighlightedTitle, Key = x.Key, Type = PageType.Other})
                          .ToList();
        }
    }
}
