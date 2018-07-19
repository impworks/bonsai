using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The manager service for handling media items.
    /// </summary>
    public class MediaManagerService
    {
        public MediaManagerService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

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
            await ValidateRequestAsync(vm).ConfigureAwait(false);

            var media = await _db.Media
                                 .GetAsync(x => x.Id == vm.Id, "Медиа-файл не найден")
                                 .ConfigureAwait(false);

            // todo: changeset

            _mapper.Map(vm, media);

            _db.MediaTags.RemoveRange(media.Tags);
            media.Tags = await DeserializeTagsAsync(vm.Tags).ConfigureAwait(false);
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

        /// <summary>
        /// Creates tag elements.
        /// </summary>
        private async Task<ICollection<MediaTag>> DeserializeTagsAsync(string raw)
        {
            var tagVms = JsonConvert.DeserializeObject<MediaTagVM[]>(raw ?? "[]");

            var relatedIds = tagVms.Where(x => x.PageId.HasValue)
                                   .Select(x => x.PageId.Value)
                                   .ToList();

            if (relatedIds.Any())
            {
                var foundPageIds = await _db.Pages
                                            .Where(x => relatedIds.Contains(x.Id))
                                            .Select(x => x.Id)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

                var hasMissingIds = relatedIds.Except(foundPageIds).Any();
                if(hasMissingIds)
                    throw new ValidationException(nameof(MediaEditorVM.Tags), "Выбранная страница не существует!");
            }

            return tagVms.Select(x => _mapper.Map<MediaTagVM, MediaTag>(x)).ToList();
        }

        /// <summary>
        /// Checks if the update request contains valid data.
        /// </summary>
        private async Task ValidateRequestAsync(MediaEditorVM vm)
        {
            var val = new Validator();

            if (!string.IsNullOrEmpty(vm.Date) && FuzzyDate.TryParse(vm.Date) == null)
                val.Add(nameof(vm.Date), "Введите корректную дату.");

            val.ThrowIfInvalid();
        }

        #endregion
    }
}
