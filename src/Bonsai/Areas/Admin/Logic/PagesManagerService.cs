using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.Logic.Validation;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The manager service for handling pages.
    /// </summary>
    public class PagesManagerService
    {
        public PagesManagerService(AppDbContext db, IMapper mapper, UserManager<AppUser> userMgr, PageValidator validator, CacheService cache)
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
            _validator = validator;
            _cache = cache;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;
        private readonly PageValidator _validator;
        private readonly CacheService _cache;

        #region Public methods

        /// <summary>
        /// Finds pages.
        /// </summary>
        public async Task<PagesListVM> GetPagesAsync(PagesListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = _db.PagesScored
                           .Include(x => x.MainPhoto)
                           .Where(x => x.IsDeleted == false);

            if(!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(x => x.Title.ToLower().Contains(request.SearchQuery.ToLower()));

            if (request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync();

            var items = await query.OrderBy(request.OrderBy, request.OrderDescending ?? false)
                                   .ProjectTo<PageScoredVM>(_mapper.ConfigurationProvider)
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync();

            return new PagesListVM
            {
                Items = items,
                PageCount = (int) Math.Ceiling((double) totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Returns the lookup of page titles.
        /// </summary>
        public async Task<IReadOnlyDictionary<Guid, PageTitleExtendedVM>> FindPagesByIdsAsync(IReadOnlyList<Guid?> pages)
        {
            return await _db.Pages
                            .Include(x => x.MainPhoto)
                            .Where(x => pages.Any(y => x.Id == y) && x.IsDeleted == false)
                            .ProjectTo<PageTitleExtendedVM>(_mapper.ConfigurationProvider)
                            .ToDictionaryAsync(x => x.Id, x => x);
        }

        /// <summary>
        /// Returns the editor's new state (blank or restored from a draft).
        /// </summary>
        public async Task<PageEditorVM> RequestCreateAsync(PageType type, ClaimsPrincipal principal)
        {
            var draft = await GetPageDraftAsync(null, principal);
            if (draft != null)
                return JsonConvert.DeserializeObject<PageEditorVM>(draft.Content);

            return new PageEditorVM {Type = type};
        }
        
        /// <summary>
        /// Creates the new page.
        /// </summary>
        public async Task<Page> CreateAsync(PageEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateRequestAsync(vm);

            var key = PageHelper.EncodeTitle(vm.Title);
            var existingRemoved = await _db.Pages
                                           .Where(x => x.Key == key && x.IsDeleted == true)
                                           .Select(x => new { x.Id })
                                           .FirstOrDefaultAsync();
            if (existingRemoved != null)
                return await UpdateAsync(vm, principal, pageId: existingRemoved.Id);

            var page = _mapper.Map<Page>(vm);
            page.Id = Guid.NewGuid();
            page.CreationDate = DateTimeOffset.Now;
            page.MainPhoto = await FindMainPhotoAsync(vm.MainPhotoKey);

            await _validator.ValidateAsync(page, vm.Facts);

            var changeset = await GetChangesetAsync(null, vm, page.Id, principal, null);
            _db.Changes.Add(changeset);

            _db.Pages.Add(page);

            var aliasValues = JsonConvert.DeserializeObject<List<string>>(vm.Aliases ?? "[]");
            if(!aliasValues.Contains(vm.Title))
                aliasValues.Add(vm.Title);

            _db.PageAliases.AddRange(
                aliasValues.Select((x, idx) =>
                    new PageAlias
                    {
                        Id = Guid.NewGuid(),
                        Page = page,
                        Key = PageHelper.EncodeTitle(x).ToLowerInvariant(),
                        Title = x,
                        Order = idx
                    }
                )
            );

            await DiscardPageDraftAsync(null, principal);

            return page;
        }

        /// <summary>
        /// Creates a default page for the user.
        /// </summary>
        public async Task<Page> CreateDefaultUserPageAsync(RegisterUserVM vm, ClaimsPrincipal principal)
        {
            var name = new[] {vm.LastName, vm.FirstName, vm.MiddleName}
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Select(x => x.Trim())
                       .JoinString(" ");

            var createVm = new PageEditorVM
            {
                Title = name,
                Description = name,
                Type = PageType.Person,
                Facts = new JObject
                {
                    ["Birth.Date"] = new JObject { ["Value"] = vm.Birthday },
                    ["Main.Name"] = new JObject
                    {
                        ["Values"] = new JArray
                        {
                            new JObject
                            {
                                ["FirstName"] = vm.FirstName,
                                ["MiddleName"] = vm.MiddleName,
                                ["LastName"] = vm.LastName
                            }
                        }
                    }

                }.ToString()
            };

            return await CreateAsync(createVm, principal);
        }

        /// <summary>
        /// Returns the original data for the editor form.
        /// </summary>
        public async Task<PageEditorVM> RequestUpdateAsync(Guid id, ClaimsPrincipal principal, bool force = false)
        {
            if (!force)
            {
                var draft = await GetPageDraftAsync(id, principal);
                if (draft != null)
                    return JsonConvert.DeserializeObject<PageEditorVM>(draft.Content);
            }

            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(x => x.MainPhoto)
                                .Include(x => x.Aliases)
                                .GetAsync(x => x.Id == id && x.IsDeleted == false, "Страница не найдена");

            return _mapper.Map<Page, PageEditorVM>(page);
        }

        /// <summary>
        /// Updates the changes to a page.
        /// </summary>
        public async Task<Page> UpdateAsync(PageEditorVM vm, ClaimsPrincipal principal, Guid? revertedChangeId = null, Guid? pageId = null)
        {
            await ValidateRequestAsync(vm);

            if (pageId == null)
                pageId = vm.Id;

            var page = await _db.Pages
                                .Include(x => x.Aliases)
                                .Include(x => x.MainPhoto)
                                .GetAsync(x => x.Id == pageId, "Страница не найдена");

            await _validator.ValidateAsync(page, vm.Facts);

            var prevVm = page.IsDeleted ? null : _mapper.Map<PageEditorVM>(page);
            var changeset = await GetChangesetAsync(prevVm, vm, pageId.Value, principal, revertedChangeId);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, page);
            page.MainPhotoId = (await FindMainPhotoAsync(vm.MainPhotoKey))?.Id;

            page.IsDeleted = false;

            await _db.PageAliases.RemoveWhereAsync(x => x.Page.Id == pageId);

            var aliasValues = JsonConvert.DeserializeObject<List<string>>(vm.Aliases ?? "[]");
            if(!aliasValues.Contains(vm.Title))
                aliasValues.Add(vm.Title);

            _db.PageAliases.AddRange(
                aliasValues.Select((x, idx) => new PageAlias
                {
                    Id = Guid.NewGuid(),
                    Key = PageHelper.EncodeTitle(x).ToLowerInvariant(),
                    Page = page,
                    Order = idx,
                    Title = x
                })
            );

            if (prevVm?.Title != vm.Title || prevVm?.Facts != vm.Facts)
            {
                _cache.Clear();
            }
            else
            {
                _cache.Remove<PageDescriptionVM>(page.Key);
                _cache.Remove<InfoBlockVM>(page.Key);
            }

            if(revertedChangeId == null)
                await DiscardPageDraftAsync(vm.Id, principal);

            return page;
        }

        /// <summary>
        /// Displays the remove confirmation form.
        /// </summary>
        public async Task<PageTitleExtendedVM> RequestRemoveAsync(Guid id)
        {
            return await _db.Pages
                            .Where(x => x.IsDeleted == false)
                            .ProjectTo<PageTitleExtendedVM>(_mapper.ConfigurationProvider)
                            .GetAsync(x => x.Id == id, "Страница не найдена");
        }

        /// <summary>
        /// Removes the page.
        /// </summary>
        public async Task<Page> RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var page = await _db.Pages
                                .GetAsync(x => x.Id == id && x.IsDeleted == false, "Страница не найдена");

            var prev = await RequestUpdateAsync(id, principal, force: true);
            var changeset = await GetChangesetAsync(prev, null, id, principal, null);
            _db.Changes.Add(changeset);

            page.IsDeleted = true;

            await _db.PageAliases.RemoveWhereAsync(x => x.Page.Id == id);

            _cache.Clear();

            return page;
        }

        /// <summary>
        /// Returns the editor state from the current draft.
        /// </summary>
        public async Task<PageDraft> GetPageDraftAsync(Guid? page, ClaimsPrincipal principal)
        {
            var userId = _userMgr.GetUserId(principal);
            var pageId = page == Guid.Empty ? null : page;
            return await _db.PageDrafts.FirstOrDefaultAsync(x => x.PageId == pageId && x.UserId == userId);
        }

        /// <summary>
        /// Updates the existing draft state.
        /// </summary>
        public async Task<PageDraftInfoVM> UpdatePageDraftAsync(PageEditorVM vm, ClaimsPrincipal principal)
        {
            var userId = _userMgr.GetUserId(principal);
            var pageId = vm.Id == Guid.Empty ? null : (Guid?) vm.Id;

            var draft = await _db.PageDrafts
                                 .FirstOrDefaultAsync(x => x.PageId == pageId && x.UserId == userId);

            if (draft == null)
            {
                draft = new PageDraft
                {
                    Id = Guid.NewGuid(),
                    PageId = pageId,
                    UserId = userId
                };
                _db.PageDrafts.Add(draft);
            }

            draft.Content = JsonConvert.SerializeObject(vm);
            draft.LastUpdateDate = DateTimeOffset.Now;

            return new PageDraftInfoVM
            {
                LastUpdateDate = draft.LastUpdateDate
            };
        }

        /// <summary>
        /// Discards the existing draft for a page.
        /// </summary>
        public async Task DiscardPageDraftAsync(Guid? pageId, ClaimsPrincipal principal)
        {
            var draft = await GetPageDraftAsync(pageId, principal);

            if (draft != null)
                _db.PageDrafts.Remove(draft);
        }

        /// <summary>
        /// Returns the preview-friendly version of the page.
        /// </summary>
        public async Task<Page> GetPageDraftPreviewAsync(Guid? pageId, ClaimsPrincipal principal)
        {
            var draft = await GetPageDraftAsync(pageId, principal);
            if (draft == null)
                return null;

            var editor = JsonConvert.DeserializeObject<PageEditorVM>(draft.Content);
            var page = _mapper.Map<Page>(editor);
            page.MainPhoto = await FindMainPhotoAsync(editor.MainPhotoKey);

            return page;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private PagesListRequestVM NormalizeListRequest(PagesListRequestVM vm)
        {
            if (vm == null)
                vm = new PagesListRequestVM();

            var orderableFields = new[] {nameof(PageScoredVM.Title), nameof(PageScoredVM.LastUpdateDate), nameof(PageScoredVM.CreationDate), nameof(PageScoredVM.CompletenessScore)};
            if (!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if (vm.Page < 0)
                vm.Page = 0;

            if (vm.OrderDescending == null)
                vm.OrderDescending = false;

            return vm;
        }

        /// <summary>
        /// Checks if the create/update request contains valid data.
        /// </summary>
        private async Task ValidateRequestAsync(PageEditorVM vm)
        {
            var val = new Validator();

            if (vm.Description == null)
                vm.Description = string.Empty;

            var key = PageHelper.EncodeTitle(vm.Title).ToLowerInvariant();
            var otherPage = await _db.PageAliases
                                     .AnyAsync(x => x.Key == key
                                                    && x.Page.Id != vm.Id);

            if (otherPage)
                val.Add(nameof(PageEditorVM.Title), "Страница с таким названием уже существует.");

            if (!string.IsNullOrEmpty(vm.Aliases))
            {
                var aliases = TryDeserialize<List<string>>(vm.Aliases)?.Select(x => x.ToLowerInvariant());
                if (aliases == null)
                {
                    val.Add(nameof(PageEditorVM.Aliases), "Ссылки указаны неверно!");
                }
                else
                {
                    var otherAliases = await _db.PageAliases
                                                .Where(x => aliases.Contains(x.Key) && x.Page.Id != vm.Id)
                                                .Select(x => x.Title)
                                                .ToListAsync();

                    if (otherAliases.Any())
                        val.Add(nameof(PageEditorVM.Aliases), "Ссылки уже заняты другими страницами: " + otherAliases.JoinString(", "));
                }
            }
                

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetChangesetAsync(PageEditorVM prev, PageEditorVM next, Guid id, ClaimsPrincipal principal, Guid? revertedId)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            var user = await _userMgr.GetUserAsync(principal, "Пользователь не найден");

            return new Changeset
            {
                Id = Guid.NewGuid(),
                RevertedChangesetId = revertedId,
                Type = ChangesetEntityType.Page,
                Date = DateTime.Now,
                EditedPageId = id,
                Author = user,
                OriginalState = prev == null ? null : JsonConvert.SerializeObject(prev),
                UpdatedState = next == null ? null : JsonConvert.SerializeObject(next),
            };
        }

        /// <summary>
        /// Finds the image to use for the page.
        /// </summary>
        private async Task<Media> FindMainPhotoAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var media = await _db.Media
                                 .FirstOrDefaultAsync(x => x.Key == key && x.IsDeleted == false);

            if(media == null)
                throw new ValidationException(nameof(PageEditorVM.MainPhotoKey), "Фотография не найдена!");

            if(media.Type != MediaType.Photo)
                throw new ValidationException(nameof(PageEditorVM.MainPhotoKey), "Медиа-файл не является фотографией!");

            return media;
        }

        /// <summary>
        /// Checks if the serialized field contains valid data.
        /// </summary>
        private T TryDeserialize<T>(string value) where T: class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}

