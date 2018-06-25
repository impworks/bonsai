using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic.Pages
{
    /// <summary>
    /// The manager service for handling pages.
    /// </summary>
    public class PagesManagerService
    {
        public PagesManagerService(AppDbContext db, IMapper mapper, UserManager<AppUser> userMgr)
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;

        #region Pages

        /// <summary>
        /// Finds pages.
        /// </summary>
        public async Task<PagesListVM> GetPagesAsync(PagesListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = _db.Pages.Include(x => x.MainPhoto).AsQueryable();

            if(!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(x => x.Title.ToLower().Contains(request.SearchQuery.ToLower()));

            if (request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            var items = await query.OrderBy(request.OrderBy, request.OrderDescending)
                                   .ProjectTo<PageTitleExtendedVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return new PagesListVM
            {
                Items = items,
                PageCount = (int) Math.Ceiling((double) totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Returns the original data for the editor form.
        /// </summary>
        public async Task<PageEditorVM> RequestUpdateAsync(Guid id)
        {
            var page = await _db.Pages
                                .AsNoTracking()
                                .Include(x => x.MainPhoto)
                                .Include(x => x.Relations)
                                .Include(x => x.Aliases)
                                .GetAsync(x => x.Id == id, "Страница не найдена")
                                .ConfigureAwait(false);

            return _mapper.Map<Page, PageEditorVM>(page);
        }

        /// <summary>
        /// Updates the changes to a page.
        /// </summary>
        public async Task UpdateAsync(PageEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateUpdateRequestAsync(vm).ConfigureAwait(false);

            var page = await _db.Pages
                                .GetAsync(x => x.Id == vm.Id, "Страница не найдена")
                                .ConfigureAwait(false);

            var changeset = await GetUpdateChangesetAsync(page, _mapper.Map<Page>(vm), principal);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, page);
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

            var orderableFields = new[] {nameof(Page.Title), nameof(Page.LastUpdateDate), nameof(Page.CreateDate)};
            if (!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if (vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        /// <summary>
        /// Checks if the request contains valid data.
        /// </summary>
        private async Task ValidateUpdateRequestAsync(PageEditorVM vm)
        {
            var val = new Validator();

            var key = PageHelper.EncodeTitle(vm.Title);
            var otherPage = await _db.PageAliases
                                     .AnyAsync(x => x.Key == key && x.Page.Id != vm.Id)
                                     .ConfigureAwait(false);

            if (otherPage)
                val.Add(nameof(PageEditorVM.Title), "Страница с таким названием уже существует.");

            val.ThrowIfInvalid();
        }

        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetUpdateChangesetAsync(Page prev, Page next, ClaimsPrincipal principal)
        {
            var userId = _userMgr.GetUserId(principal);
            var user = await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден").ConfigureAwait(false);

            // todo: diff!

            return new Changeset
            {
                Id = Guid.NewGuid(),
                Type = ChangesetEntityType.Page,
                Date = DateTime.Now,
                SourceEntityId = prev.Id,
                SourceDiff = "{}",
                Author = user
            };
        }

        #endregion
    }
}

