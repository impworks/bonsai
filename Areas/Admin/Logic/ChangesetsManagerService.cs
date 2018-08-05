using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// The service for searching and displaying changesets.
    /// </summary>
    public class ChangesetsManagerService
    {
        public ChangesetsManagerService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        #region Public methods

        /// <summary>
        /// Finds changesets.
        /// </summary>
        public async Task<ChangesetsListVM> GetChangesetsAsync(ChangesetsListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = from c in _db.Changes.Include(x => x.Author)
                        join p in _db.Pages on c.EntityId equals p.Id
                        join m in _db.Media on c.EntityId equals m.Id
                        select new
                        {
                            Changeset = c,
                            Page = p,
                            Media = m
                        };

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var search = request.SearchQuery.ToLower();
                query = query.Where(x => (x.Page != null && x.Page.Title.ToLower().Contains(search))
                                         || x.Media != null && x.Media.Title.ToLower().Contains(search));
            }

            if (request.EntityTypes?.Length > 0)
                query = query.Where(x => request.EntityTypes.Contains(x.Changeset.Type));

            if (request.EntityId != null)
                query = query.Where(x => x.Changeset.EntityId == request.EntityId);

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            if (request.OrderBy == nameof(Changeset.Author))
                query = query.OrderBy(x => x.Changeset.Author.UserName, request.OrderDescending);
            else
                query = query.OrderBy(x => x.Changeset.Date, request.OrderDescending);

            var items = await query.Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .Select(x => new ChangesetTitleVM
                                   {
                                       Id = x.Changeset.Id,
                                       Date = x.Changeset.Date,
                                       ChangeType = ChangesetTitleVM.GetChangeType(x.Changeset),
                                       Author = x.Changeset.Author.FirstName + " " + x.Changeset.Author.LastName,
                                       EntityId = x.Changeset.EntityId,
                                       EntityType = x.Changeset.Type,
                                       EntityTitle = x.Page != null
                                           ? x.Page.Title
                                           : x.Media.Title,
                                       EntityThumbnailUrl = x.Media != null
                                           ? x.Media.FilePath
                                           : (x.Page.MainPhoto != null
                                               ? x.Page.MainPhoto.FilePath
                                               : null),
                                       PageType = x.Page != null
                                           ? x.Page.Type
                                           : (PageType?) null
                                   })
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            return new ChangesetsListVM
            {
                Items = items,
                PageCount = (int)Math.Ceiling((double)totalCount / PageSize),
                Request = request
            };
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private ChangesetsListRequestVM NormalizeListRequest(ChangesetsListRequestVM vm)
        {
            if(vm == null)
                vm = new ChangesetsListRequestVM();

            var orderableFields = new[] { nameof(Changeset.Date), nameof(Changeset.Author) };
            if(!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if(vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        #endregion
    }
}
