using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Code.Services.Elastic;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for various lookups required by the interface.
    /// </summary>
    public class SuggestService
    {
        public SuggestService(AppDbContext db, ElasticService elastic, IUrlHelper urlHelper)
        {
            _db = db;
            _elastic = elastic;
            _url = urlHelper;
        }

        private readonly AppDbContext _db;
        private readonly ElasticService _elastic;
        private readonly IUrlHelper _url;

        /// <summary>
        /// Suggests pages of specified types.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleExtendedVM>> SuggestPagesAsync(string query, IReadOnlyList<PageType> types = null)
        {
            var search = await _elastic.SearchAutocompleteAsync(query, types, 100);

            var ids = search.Select(x => x.Id).ToList();
            var idsOrder = ids.Select((val, id) => new { Value = val, Index = id })
                              .ToDictionary(x => x.Value, x => x.Index);

            var pages = await _db.Pages
                                 .Where(x => ids.Contains(x.Id))
                                 .ProjectTo<PageTitleExtendedVM>()
                                 .ToListAsync();

            // URL is global for easier usage in JSON
            foreach (var page in pages)
                page.MainPhotoPath = GetFullThumbnailPath(page);

            return pages.OrderBy(x => idsOrder[x.Id]).ToList();
        }

        #region Helpers

        /// <summary>
        /// Returns the full path for the page's main photo.
        /// </summary>
        private string GetFullThumbnailPath(PageTitleExtendedVM page)
        {
            return _url.Content(PageHelper.GetPageImageUrl(page.Type, page.MainPhotoPath));
        }

        #endregion
    }
}
