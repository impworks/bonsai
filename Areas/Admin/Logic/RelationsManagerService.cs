using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Data;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for managing relations.
    /// </summary>
    public class RelationsManagerService
    {
        public RelationsManagerService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Returns the found relations.
        /// </summary>
        public async Task<RelationsListVM> GetRelationsAsync(RelationsListRequestVM request)
        {
            const int PageSize = 50;

            request = NormalizeListRequest(request);

            var query = _db.Relations.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var req = request.SearchQuery.ToLower();
                query = query.Where(x => x.Destination.Title.ToLower().Contains(req)
                                         || x.Source.Title.ToLower().Contains(req)
                                         || x.Event.Title.ToLower().Contains(req));
            }

            if (request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            if (request.OrderBy == nameof(RelationTitleVM.Destination))
                query = query.OrderBy(x => x.Destination.Title, request.OrderDescending);
            else if (request.OrderBy == nameof(RelationTitleVM.Source))
                query = query.OrderBy(x => x.Source.Title, request.OrderDescending);
            else
                query = query.OrderBy(x => x.Event.Title, request.OrderDescending);

            var items = await query.ProjectTo<RelationTitleVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return new RelationsListVM
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
        private RelationsListRequestVM NormalizeListRequest(RelationsListRequestVM vm)
        {
            if (vm == null)
                vm = new RelationsListRequestVM();

            var orderableFields = new[] {nameof(RelationTitleVM.Source), nameof(RelationTitleVM.Destination), nameof(RelationTitleVM.Event)};
            if (!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if (vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        #endregion
    }
}
