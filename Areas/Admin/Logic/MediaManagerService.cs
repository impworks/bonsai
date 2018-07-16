using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The manager service for handling media items.
    /// </summary>
    public class MediaManagerService
    {
        public MediaManagerService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        #region Public methods

        /// <summary>
        /// Finds media files.
        /// </summary>
        public async Task<MediaListVM> GetMediaAsync(MediaListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = _db.Media.Include(x => x.Tags).AsQueryable();

            if(!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(x => x.Title.ToLower().Contains(request.SearchQuery.ToLower()));

            if(request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            var items = await query.OrderBy(request.OrderBy, request.OrderDescending)
                                   .ProjectTo<MediaThumbnailExtendedVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return new MediaListVM
            {
                Items = items,
                PageCount = (int)Math.Ceiling((double)totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Uploads a new media file.
        /// </summary>
        public async Task<MediaUploadResultVM> UploadAsync(MediaUploadRequestVM vm, ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the media data.
        /// </summary>
        public async Task UpdateAsync(MediaEditorVM vm, ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the media file.
        /// </summary>
        public async Task RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private MediaListRequestVM NormalizeListRequest(MediaListRequestVM vm)
        {
            if(vm == null)
                vm = new MediaListRequestVM();

            var orderableFields = new[] { nameof(Media.Title), nameof(Media.UploadDate), nameof(Media.Date) };
            if(!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if(vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        #endregion
    }
}
