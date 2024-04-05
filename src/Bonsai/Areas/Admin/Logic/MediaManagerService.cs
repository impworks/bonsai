﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        public MediaManagerService(
            AppDbContext db,
            UserManager<AppUser> userMgr,
            IMapper mapper,
            IWebHostEnvironment env,
            IEnumerable<IMediaHandler> mediaHandlers,
            IBackgroundJobService jobs,
            CacheService cache
        )
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
            _env = env;
            _jobs = jobs;
            _mediaHandlers = mediaHandlers.ToList();
            _cache = cache;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;
        private readonly IWebHostEnvironment _env;
        private readonly IBackgroundJobService _jobs;
        private readonly IReadOnlyList<IMediaHandler> _mediaHandlers;
        private readonly CacheService _cache;

        #region Public methods

        /// <summary>
        /// Finds media files.
        /// </summary>
        public async Task<MediaListVM> GetMediaAsync(MediaListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var result = new MediaListVM { Request = request };
            await FillAdditionalDataAsync(request, result);

            var query = _db.Media
                           .Include(x => x.Tags)
                           .Where(x => x.IsDeleted == false);

            if(!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(x => x.NormalizedTitle.Contains(PageHelper.NormalizeTitle(request.SearchQuery)));

            if (request.EntityId != null)
                query = query.Where(x => x.Tags.Any(y => y.ObjectId == request.EntityId));

            if(request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync();
            result.PageCount = (int) Math.Ceiling((double) totalCount / PageSize);

            var isDesc = request.OrderDescending ?? false;
            if (request.OrderBy == nameof(Media.Tags))
                query = query.OrderBy(x => x.Tags.Count(y => y.Type == MediaTagType.DepictedEntity), isDesc);
            else if (request.OrderBy == nameof(Media.Title))
                query = query.OrderBy(x => x.Title, isDesc).ThenBy(x => x.UploadDate, isDesc);
            else
                query = query.OrderBy(request.OrderBy, isDesc);

            result.Items = await query.ProjectToType<MediaThumbnailExtendedVM>(_mapper.Config)
                                      .Skip(PageSize * request.Page)
                                      .Take(PageSize)
                                      .ToListAsync();
            return result;
        }

        /// <summary>
        /// Uploads a new media file.
        /// </summary>
        public async Task<MediaUploadResultVM> UploadAsync(MediaUploadRequestVM vm, IFormFile file, ClaimsPrincipal principal)
        {
            var id = Guid.NewGuid();
            var key = PageHelper.GetMediaKey(id);

            var handler = _mediaHandlers.FirstOrDefault(x => x.SupportedMimeTypes.Contains(file.ContentType));
            if(handler == null)
                throw new UploadException(Texts.Admin_Media_UnknownFileType);

            var user = await _userMgr.GetUserAsync(principal, Texts.Admin_Users_NotFound);
            var paths = await SaveUploadAsync(file, key, handler);
            var tags = await GetTagsForUploadedMedia(vm);
            var meta = await handler.ExtractMetadataAsync(paths.LocalPath, file.ContentType);

            var title = vm.UseFileNameAsTitle ? SanitizeFileName(file.FileName) : vm.Title;
            var media = new Media
            {
                Id = id,
                Key = key,
                Type = handler.MediaType,
                MimeType = file.ContentType,
                Title = title,
                NormalizedTitle = PageHelper.NormalizeTitle(title),
                FilePath = paths.UrlPath,
                UploadDate = DateTimeOffset.Now,
                Uploader = user,
                IsProcessed = handler.IsImmediate,
                Date = FuzzyDate.TryParse(vm.Date) != null
                    ? vm.Date
                    : meta?.Date?.Date.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture),
                Tags = tags
            };

            _db.Media.Add(media);

            var changeset = await GetChangesetAsync(null, _mapper.Map<MediaEditorVM>(media), id, principal, null);
            _db.Changes.Add(changeset);

            return _mapper.Map<MediaUploadResultVM>(media);
        }

        /// <summary>
        /// Returns the media file editor.
        /// </summary>
        public async Task<MediaEditorVM> RequestUpdateAsync(Guid id, bool includeDeleted = false)
        {
            var media = await _db.Media
                                 .AsNoTracking()
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == id && (x.IsDeleted == false || includeDeleted),
                                           Texts.Admin_Media_NotFound);

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
        public async Task UpdateAsync(MediaEditorVM vm, ClaimsPrincipal principal, Guid? revertedId = null)
        {
            await ValidateRequestAsync(vm);

            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == vm.Id && (x.IsDeleted == false || revertedId != null),
                                           Texts.Admin_Media_NotFound);

            var prevTags = media.Tags.ToList();
            var prevVm = media.IsDeleted ? null : await RequestUpdateAsync(vm.Id, revertedId != null);
            var changeset = await GetChangesetAsync(prevVm, vm, vm.Id, principal, revertedId);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, media);
            media.NormalizedTitle = PageHelper.NormalizeTitle(vm.Title);

            if(revertedId != null)
                media.IsDeleted = false;

            _db.MediaTags.RemoveRange(media.Tags);
            foreach (var tag in DeserializeTags(vm))
            {
                tag.Media = media;
                _db.MediaTags.Add(tag);
            }

            await ClearCacheAsync(media, prevTags);
        }

        /// <summary>
        /// Returns the confirmation info for the media.
        /// </summary>
        public async Task<RemoveEntryInfoVM<string>> RequestRemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var media = await _db.Media
                                 .AsNoTracking()
                                 .GetAsync(x => x.Id == id && x.IsDeleted == false, Texts.Admin_Media_NotFound);

            var isAdmin = await _userMgr.IsInRoleAsync(principal, UserRole.Admin);
            
            return new RemoveEntryInfoVM<string>
            {
                Entry = MediaPresenterService.GetSizedMediaPath(media.FilePath, MediaSize.Small),
                CanRemoveCompletely = isAdmin
            };
        }

        /// <summary>
        /// Removes the media file.
        /// </summary>
        public async Task RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == id && x.IsDeleted == false, Texts.Admin_Media_NotFound);

            var prevState = await RequestUpdateAsync(id);
            var changeset = await GetChangesetAsync(prevState, null, id, principal, null);
            _db.Changes.Add(changeset);

            media.IsDeleted = true;

            await ClearCacheAsync(media);
        }

        /// <summary>
        /// Removes the media file irreversibly, including the media on disk.
        /// </summary>
        public async Task RemoveCompletelyAsync(Guid id, ClaimsPrincipal principal)
        {
            if (await _userMgr.IsInRoleAsync(principal, UserRole.Admin) == false)
                throw new OperationException(Texts.Admin_Users_Forbidden);
            
            var media = await _db.Media
                                 .Include(x => x.Tags)
                                 .GetAsync(x => x.Id == id, Texts.Admin_Media_NotFound);
            
            _db.MediaTags.RemoveRange(media.Tags);

            await _db.Changes.RemoveWhereAsync(x => x.EditedMediaId == id);
            
            await foreach (var page in _db.Pages.WhereAsync(x => x.MainPhotoId == id))
                page.MainPhotoId = null;
            
            foreach(var size in new [] { MediaSize.Small, MediaSize.Original, MediaSize.Medium, MediaSize.Large })
                File.Delete(_env.GetMediaPath(media, size));

            _db.Media.Remove(media);
            
            _cache.Clear();
        }

        /// <summary>
        /// Returns the thumbnails for the media files.
        /// </summary>
        public async Task<IReadOnlyList<MediaUploadResultVM>> GetThumbnailsAsync(IEnumerable<Guid> ids)
        {
            return await _db.Media
                            .Where(x => ids.Contains(x.Id))
                            .ProjectToType<MediaUploadResultVM>(_mapper.Config)
                            .ToListAsync();
        }

        /// <summary>
        /// Returns the ID of the chronologically first media without tags in the database.
        /// </summary>
        public async Task<Guid?> GetNextUntaggedMediaAsync()
        {
            return await _db.Media
                            .Where(x => !x.Tags.Any(y => y.Type == MediaTagType.DepictedEntity))
                            .Where(x => x.IsProcessed && !x.IsDeleted)
                            .OrderByDescending(x => x.UploadDate)
                            .Select(x => (Guid?) x.Id)
                            .FirstOrDefaultAsync();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private MediaListRequestVM NormalizeListRequest(MediaListRequestVM vm)
        {
            if(vm == null)
                vm = new MediaListRequestVM { OrderDescending = true };

            var orderableFields = new[] { nameof(Media.UploadDate), nameof(Media.Date), nameof(Media.Title), nameof(Media.Tags) };
            if(!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if(vm.Page < 0)
                vm.Page = 0;

            if (vm.OrderDescending == null)
                vm.OrderDescending = true;

            return vm;
        }

        /// <summary>
        /// Creates tag elements.
        /// </summary>
        private ICollection<MediaTag> DeserializeTags(MediaEditorVM vm)
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
                val.Add(nameof(vm.Date), Texts.Admin_Validation_IncorrectDate);

            var depictedIds = JsonConvert.DeserializeObject<IEnumerable<MediaTagVM>>(vm.DepictedEntities ?? "[]")
                                         .Select(x => x.PageId)
                                         .ToList();

            var locId = vm.Location.TryParse<Guid?>();
            var evtId = vm.Event.TryParse<Guid?>();
            var tagIds = depictedIds.Concat(new[] {locId, evtId})
                                    .Where(x => x != null)
                                    .Select(x => x.Value)
                                    .ToList();

            if (tagIds.Any())
            {
                var existing = await _db.Pages
                                        .Where(x => tagIds.Contains(x.Id) && !x.IsDeleted)
                                        .ToHashSetAsync(x => x.Id);

                if (depictedIds.Any(x => x != null && !existing.Contains(x.Value)))
                    val.Add(nameof(vm.DepictedEntities), Texts.Admin_Pages_NotFound);

                if (locId != null && !existing.Contains(locId.Value))
                    val.Add(nameof(vm.Location), Texts.Admin_Pages_NotFound);

                if (evtId != null && !existing.Contains(evtId.Value))
                    val.Add(nameof(vm.Event), Texts.Admin_Pages_NotFound);
            }

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Saves an uploaded file to disk.
        /// </summary>
        private async Task<(string LocalPath, string UrlPath)> SaveUploadAsync(IFormFile file, string key, IMediaHandler handler)
        {
            var ext = Path.GetExtension(file.FileName);
            var fileName = key + ext;
            var filePath = Path.Combine(_env.WebRootPath, "media", fileName);

            await using (var localStream = new FileStream(filePath, FileMode.CreateNew))
            await using (var sourceStream = file.OpenReadStream())
                await sourceStream.CopyToAsync(localStream);

            using(var frame = await handler.ExtractThumbnailAsync(filePath, file.ContentType))
                await MediaHandlerHelper.CreateThumbnailsAsync(filePath, frame);

            return (filePath, $"~/media/{fileName}");
        }

        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetChangesetAsync(MediaEditorVM prev, MediaEditorVM next, Guid id, ClaimsPrincipal principal, Guid? revertedId)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            var user = await _userMgr.GetUserAsync(principal, Texts.Admin_Users_NotFound);

            return new Changeset
            {
                Id = Guid.NewGuid(),
                RevertedChangesetId = revertedId,
                ChangeType = ChangesetHelper.GetChangeType(prev, next, revertedId),
                EntityType = ChangesetEntityType.Media,
                Date = DateTime.Now,
                EditedMediaId = id,
                Author = user,
                UpdatedState = next == null ? null : JsonConvert.SerializeObject(next),
            };
        }

        /// <summary>
        /// Clears the related pages from cache.
        /// </summary>
        private async Task ClearCacheAsync(Media media, IEnumerable<MediaTag> prevTags = null)
        {
            _cache.Remove<MediaVM>(media.Key);

            var tags = prevTags == null ? media.Tags : media.Tags.Concat(prevTags);
            var tagIds = tags.Select(x => x.ObjectId).ToList();
            var tagKeys = await _db.Pages
                                   .Where(x => tagIds.Contains(x.Id))
                                   .Select(x => x.Key)
                                   .ToListAsync();

            foreach (var key in tagKeys)
                _cache.Remove<PageMediaVM>(key);

            var pagesWithMain = await _db.Pages
                                         .Where(x => x.MainPhotoId == media.Id)
                                         .Select(x => x.Key)
                                         .ToListAsync();

            foreach (var key in pagesWithMain)
            {
                _cache.Remove<PageMediaVM>(key);
                _cache.Remove<PageDescriptionVM>(key);
                _cache.Remove<InfoBlockVM>(key);
            }
        }
        
        /// <summary>
        /// Loads extra data for the filter.
        /// </summary>
        private async Task FillAdditionalDataAsync(MediaListRequestVM request, MediaListVM data)
        {
            if (request.EntityId != null)
            {
                var title = await _db.Pages
                                     .Where(x => x.Id == request.EntityId)
                                     .Select(x => x.Title)
                                     .FirstOrDefaultAsync();

                if (title != null)
                    data.EntityTitle = title;
                else
                    request.EntityId = null;
            }
        }

        /// <summary>
        /// Creates event \ location tags for the uploaded media.
        /// </summary>
        private async Task<List<MediaTag>> GetTagsForUploadedMedia(MediaUploadRequestVM vm)
        {
            var result = new List<MediaTag>();

            var locId = vm.Location.TryParse<Guid>();
            var evtId = vm.Event.TryParse<Guid>();

            var tagIds = new[] {locId, evtId}
                         .Where(x => x != Guid.Empty)
                         .ToList();

            var existing = tagIds.Any()
                ? await _db.Pages
                           .Where(x => tagIds.Contains(x.Id) && !x.IsDeleted)
                           .ToHashSetAsync(x => x.Id)
                : null;

            TryAddTag(vm.Location, locId, MediaTagType.Location);
            TryAddTag(vm.Event, evtId, MediaTagType.Event);

            return result;

            void TryAddTag(string title, Guid id, MediaTagType type)
            {
                if (string.IsNullOrEmpty(title))
                    return;

                var tag = new MediaTag { Type = type };

                if (existing?.Contains(id) == true)
                    tag.ObjectId = id;
                else
                    tag.ObjectTitle = title;

                result.Add(tag);
            }
        }

        /// <summary>
        /// Returns the "cleaned-up" file name.
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            var extless = Path.GetFileNameWithoutExtension(fileName);
            return extless.ReplaceRegex("[\\W_]", " ")
                          .ReplaceRegex("\\s{2,}", " ");
        }

        #endregion
    }
}
