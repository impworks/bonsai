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

            request = ValidateRequest(request);

            var query = (IQueryable<Page>) _db.Pages.Include(x => x.MainPhoto);

            if(!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(x => x.Title.Contains(request.SearchQuery));

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
        private PagesListRequestVM ValidateRequest(PagesListRequestVM vm)
        {
            if(vm == null)
                vm = new PagesListRequestVM {OrderBy = nameof(Page.Title)};

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
