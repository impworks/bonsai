using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services.Search;
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
        public SuggestService(AppDbContext db, ISearchEngine search, IUrlHelper urlHelper, IMapper mapper)
        {
            _db = db;
            _search = search;
            _url = urlHelper;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly ISearchEngine _search;
        private readonly IUrlHelper _url;
        private readonly IMapper _mapper;

        /// <summary>
        /// Suggests pages of specified types.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleExtendedVM>> SuggestPagesAsync(
            PickRequestVM<PageType> request,
            Func<IReadOnlyList<Guid>, IReadOnlyList<Guid>> extraFilter = null
        )
        {
            var search = await _search.SuggestAsync(request.Query, request.Types, 100);

            var ids = (IReadOnlyList<Guid>) search.Select(x => x.Id).ToList();

            if (extraFilter != null)
                ids = extraFilter(ids);

            var pages = await _db.Pages
                                 .Where(x => ids.Contains(x.Id))
                                 .ProjectTo<PageTitleExtendedVM>(_mapper.ConfigurationProvider)
                                 .ToDictionaryAsync(x => x.Id, x => x);

            // URL is global for easier usage in JSON
            foreach (var page in pages.Values)
                page.MainPhotoPath = GetFullThumbnailPath(page);

            return ids.Select(x => pages[x]).ToList();
        }

        /// <summary>
        /// Suggests pages for the relations editor.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleExtendedVM>> SuggestRelationPagesAsync(RelationSuggestQueryVM request)
        {
            if (request == null)
                return null;

            var subRequest = new PickRequestVM<PageType> {Query = request.Query, Types = request.Types};

            if (request.DestinationId == null && request.SourceId == null)
                return await SuggestPagesAsync(subRequest);

            var queryRoot = _db.Relations
                               .Where(x => x.IsDeleted == false);

            var idQuery = request.DestinationId != null
                ? queryRoot.Where(x => x.DestinationId == request.DestinationId)
                           .Select(x => x.SourceId)
                : queryRoot.Where(x => x.SourceId == request.SourceId)
                           .Select(x => x.DestinationId);

            var existingIds = await idQuery.ToHashSetAsync();

            var selfId = request.DestinationId ?? request.SourceId ?? Guid.Empty;
            existingIds.Add(selfId);

            return await SuggestPagesAsync(
                new PickRequestVM<PageType> { Query = request.Query, Types = request.Types },
                ids => ids.Where(id => !existingIds.Contains(id)).ToList()
            );
        }

        /// <summary>
        /// Returns the pickable pages.
        /// </summary>
        public async Task<IReadOnlyList<PageTitleExtendedVM>> GetPickablePagesAsync(PickRequestVM<PageType> request)
        {
            var q = _db.Pages.AsQueryable();

            if (!string.IsNullOrEmpty(request.Query))
            {
                var queryLower = request.Query.ToLower();
                q = q.Where(x => x.Aliases.Any(y => y.Title.ToLower().Contains(queryLower)));
            }

            if (request.Types?.Length > 0)
                q = q.Where(x => request.Types.Contains(x.Type));

            var count = Math.Clamp(request.Count ?? 100, 1, 100);
            var offset = Math.Max(request.Offset ?? 0, 0);

            var vms = await q.OrderBy(x => x.Title)
                             .Skip(offset)
                             .Take(count)
                             .ProjectTo<PageTitleExtendedVM>(_mapper.ConfigurationProvider)
                             .ToListAsync();

            foreach (var vm in vms)
                vm.MainPhotoPath = GetFullThumbnailPath(vm);

            return vms;
        }

        /// <summary>
        /// Returns the pickable media.
        /// </summary>
        public async Task<IReadOnlyList<MediaThumbnailVM>> GetPickableMediaAsync(PickRequestVM<MediaType> request)
        {
            var q = _db.Media.AsNoTracking();

            if (!string.IsNullOrEmpty(request.Query))
            {
                var queryLower = request.Query.ToLower();
                q = q.Where(x => x.Title.ToLower().Contains(queryLower));
            }

            if (request.Types?.Length > 0)
                q = q.Where(x => request.Types.Contains(x.Type));

            var count = Math.Clamp(request.Count ?? 100, 1, 100);
            var offset = Math.Max(request.Offset ?? 0, 0);

            var media = await q.OrderByDescending(x => x.UploadDate)
                               .Skip(offset)
                               .Take(count)
                               .ToListAsync();

            var vms = media.Select(x => MediaPresenterService.GetMediaThumbnail(x, MediaSize.Small)).ToList();

            foreach (var vm in vms)
                vm.ThumbnailUrl = _url.Content(vm.ThumbnailUrl);

            return vms;
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

