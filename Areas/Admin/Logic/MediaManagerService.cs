using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MediaTagVM = Bonsai.Areas.Admin.ViewModels.Media.MediaTagVM;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The manager service for handling media items.
    /// </summary>
    public class MediaManagerService
    {
        public MediaManagerService(AppDbContext db, UserManager<AppUser> userMgr, IMapper mapper, IHostingEnvironment env)
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
            _env = env;
            _mediaHandlers = GetMediaHandlers().ToList();
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;
        private readonly IHostingEnvironment _env;
        private readonly IReadOnlyList<IMediaHandler> _mediaHandlers;

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

            var totalCount = await query.CountAsync();

            var items = await query.Where(x => x.IsDeleted == false)
                                   .OrderBy(request.OrderBy, request.OrderDescending)
                                   .ProjectTo<MediaThumbnailExtendedVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync();

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
            var id = Guid.NewGuid();
            var key = PageHelper.GetMediaKey(id);

            var handler = _mediaHandlers.FirstOrDefault(x => x.SupportedMimeTypes.Contains(vm.MimeType));
            if(handler == null)
                throw new UploadException("Неизвестный тип файла!");

            var userId = _userMgr.GetUserId(principal);
            var user = await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден");

            var filePath = await SaveUploadAsync(vm, key, handler);

            var media = new Media
            {
                Id = id,
                Key = key,
                Type = handler.MediaType,
                MimeType = vm.MimeType,
                FilePath = filePath,
                UploadDate = DateTimeOffset.Now,
                Uploader = user,
            };

            _db.Media.Add(media);

            var changeset = await GetChangesetAsync(null, _mapper.Map<MediaEditorVM>(media), id, principal);
            _db.Changes.Add(changeset);

            return new MediaUploadResultVM
            {
                Id = media.Id,
                Key = media.Key,
                ThumbnailPath = MediaPresenterService.GetSizedMediaPath(filePath, MediaSize.Small)
            };
        }

        /// <summary>
        /// Returns the media file editor.
        /// </summary>
        public async Task<MediaEditorVM> RequestUpdateAsync(Guid id)
        {
            var media = await _db.Media
                                 .AsNoTracking()
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == id && x.IsDeleted == false, "Медиа-файл не найден");

            var taggedIds = media.Tags
                                 .Where(x => x.ObjectId != null)
                                 .Select(x => x.ObjectId.Value)
                                 .ToList();

            var tagNames = await _db.Pages
                                    .Where(x => taggedIds.Contains(x.Id) && x.IsDeleted == false)
                                    .ToDictionaryAsync(x => x.Id, x => x.Title);

            var vm = _mapper.Map<MediaEditorVM>(media);
            vm.Location = GetTagValue(MediaTagType.Location);
            vm.Event = GetTagValue(MediaTagType.Event);
            vm.DepictedEntities = JsonConvert.SerializeObject(
                media.Tags.Where(x => x.Type == MediaTagType.DepictedEntity)
                     .Select(x => new MediaTagVM
                     {
                         Coordinates = x.Coordinates,
                         PageId = x.ObjectId,
                         ObjectTitle = tagNames.TryGetValue(x.ObjectId ?? Guid.Empty)
                     })
            );

            return vm;

            string GetTagValue(MediaTagType type)
            {
                var tag = media.Tags.FirstOrDefault(x => x.Type == type);
                return tag?.ObjectId?.ToString() ?? tag?.ObjectTitle;
            }
        }

        /// <summary>
        /// Updates the media data.
        /// </summary>
        public async Task UpdateAsync(MediaEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateRequestAsync(vm);

            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == vm.Id && x.IsDeleted == false, "Медиа-файл не найден");

            var prevState = await RequestUpdateAsync(vm.Id);
            var changeset = await GetChangesetAsync(prevState, vm, vm.Id, principal);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, media);

            _db.MediaTags.RemoveRange(media.Tags);
            media.Tags = await DeserializeTagsAsync(vm);
        }

        /// <summary>
        /// Returns the confirmation info for the media.
        /// </summary>
        public async Task<MediaThumbnailExtendedVM> RequestRemoveAsync(Guid id)
        {
            var media = await _db.Media
                                 .AsNoTracking()
                                 .GetAsync(x => x.Id == id && x.IsDeleted == false, "Медиа-файл не найден");

            return _mapper.Map<MediaThumbnailExtendedVM>(media);
        }

        /// <summary>
        /// Removes the media file.
        /// </summary>
        public async Task RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var media = await _db.Media
                                 .GetAsync(x => x.Id == id && x.IsDeleted == false, "Медиа-файл не найден");

            var prevState = await RequestUpdateAsync(id);
            var changeset = await GetChangesetAsync(prevState, null, id, principal);
            _db.Changes.Add(changeset);

            media.IsDeleted = true;
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

            var orderableFields = new[] { nameof(Media.UploadDate), nameof(Media.Date) };
            if(!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if(vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        /// <summary>
        /// Creates tag elements.
        /// </summary>
        private async Task<ICollection<MediaTag>> DeserializeTagsAsync(MediaEditorVM vm)
        {
            var tags = JsonConvert.DeserializeObject<IEnumerable<MediaTagVM>>(vm.DepictedEntities ?? "[]")
                                  .Select(x => new MediaTag
                                  {
                                      Id = Guid.NewGuid(),
                                      Type = MediaTagType.DepictedEntity,
                                      Coordinates = x.Coordinates,
                                      ObjectId = x.PageId,
                                      ObjectTitle = x.PageId == null ? x.ObjectTitle : null
                                  })
                                  .ToList();
            
            TryParseTag(vm.Location, MediaTagType.Location);
            TryParseTag(vm.Event, MediaTagType.Event);

            return tags;

            void TryParseTag(string source, MediaTagType type)
            {
                if (string.IsNullOrEmpty(source))
                    return;

                var id = source.TryParse<Guid?>();
                tags.Add(new MediaTag
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    ObjectId = id,
                    ObjectTitle = id == null ? source : null
                });
            }
        }

        /// <summary>
        /// Checks if the update request contains valid data.
        /// </summary>
        private async Task ValidateRequestAsync(MediaEditorVM vm)
        {
            var val = new Validator();

            if (!string.IsNullOrEmpty(vm.Date) && FuzzyDate.TryParse(vm.Date) == null)
                val.Add(nameof(vm.Date), "Введите корректную дату.");

            var depictedIds = JsonConvert.DeserializeObject<IEnumerable<MediaTagVM>>(vm.DepictedEntities ?? "[]")
                                         .Select(x => x.PageId)
                                         .ToList();

            var locId = vm.Location.TryParse<Guid?>();
            var evtId = vm.Event.TryParse<Guid?>();
            var tagIds = depictedIds.Concat(new[] {locId, evtId})
                                    .Where(x => x != null)
                                    .Select(x => x.Value)
                                    .ToList();

            var existing = await _db.Pages
                                    .Where(x => tagIds.Contains(x.Id) && !x.IsDeleted)
                                    .ToDictionaryAsync(x => x.Id, x => true);

            if (depictedIds.Any(x => x != null && !existing.ContainsKey(x.Value)))
                val.Add(nameof(vm.DepictedEntities), "Страница не существует!");

            if (locId != null && !existing.ContainsKey(locId.Value))
                val.Add(nameof(vm.Location), "Страница не существует!");

            if (evtId != null && !existing.ContainsKey(evtId.Value))
                val.Add(nameof(vm.Event), "Страница не существует!");

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Saves an uploaded file to disk.
        /// </summary>
        private async Task<string> SaveUploadAsync(MediaUploadRequestVM vm, string key, IMediaHandler handler)
        {
            var ext = Path.GetExtension(vm.Name);
            var fileName = key + ext;
            var filePath = Path.Combine(_env.WebRootPath, "media", fileName);

            using (var fs = new FileStream(filePath, FileMode.CreateNew))
                await vm.Data.CopyToAsync(fs);

            MediaHandlerHelper.CreateThumbnails(filePath, vm.MimeType, handler);

            return $"~/media/{fileName}";
        }

        /// <summary>
        /// Returns the list of known media handlers.
        /// </summary>
        private IEnumerable<IMediaHandler> GetMediaHandlers()
        {
            yield return new PhotoMediaHandler();
        }

        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetChangesetAsync(MediaEditorVM prev, MediaEditorVM next, Guid id, ClaimsPrincipal principal)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            var userId = _userMgr.GetUserId(principal);
            var user = await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден");

            return new Changeset
            {
                Id = Guid.NewGuid(),
                Type = ChangesetEntityType.Media,
                Date = DateTime.Now,
                EditedMediaId = id,
                Author = user,
                OriginalState = prev == null ? null : JsonConvert.SerializeObject(prev),
                UpdatedState = next == null ? null : JsonConvert.SerializeObject(next),
            };
        }

        #endregion
    }
}
