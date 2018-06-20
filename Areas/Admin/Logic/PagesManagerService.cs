using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Dashboard;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Impworks.Utils.Linq;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The manager service for handling pages.
    /// </summary>
    public class PagesManagerService
    {
        public PagesManagerService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

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

        #endregion
    }
}
