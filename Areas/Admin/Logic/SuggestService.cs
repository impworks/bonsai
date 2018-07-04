using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Elastic;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for various lookups required by the interface.
    /// </summary>
    public class SuggestService
    {
        public SuggestService(AppDbContext db, ElasticService elastic)
        {
            _db = db;
            _elastic = elastic;
        }

        private readonly AppDbContext _db;
        private readonly ElasticService _elastic;

        /// <summary>
        /// Suggests pages for a relation.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleExtendedVM>> SuggestRelatedPagesAsync(string query, PageType pageType, RelationType relType)
        {
            var allowedPageTypes = RelationHelper.SuggestPageTypes(pageType, relType);
            var search = await _elastic.SearchAutocompleteAsync(query, allowedPageTypes, 100).ConfigureAwait(false);

            var ids = search.Select(x => x.Id).ToList();
            var idsOrder = ids.Select((val, id) => new {Value = val, Index = id})
                              .ToDictionary(x => x.Value, x => x.Index);

            var pages = await _db.Pages
                                 .Where(x => ids.Contains(x.Id))
                                 .ProjectTo<PageTitleExtendedVM>()
                                 .ToListAsync()
                                 .ConfigureAwait(false);

            return pages.OrderBy(x => idsOrder[x.Id]).ToList();
        }
    }
}
