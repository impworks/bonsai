using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// The service for searching and displaying changesets.
    /// </summary>
    public class ChangesetsManagerService
    {
        public ChangesetsManagerService(IEnumerable<IChangesetRenderer> renderers, AppDbContext db)
        {
            _db = db;
            _renderers = renderers.ToDictionary(x => x.EntityType, x => x);
        }

        private readonly AppDbContext _db;
        private IReadOnlyDictionary<ChangesetEntityType, IChangesetRenderer> _renderers;

        #region Public methods

        /// <summary>
        /// Finds changesets.
        /// </summary>
        public async Task<ChangesetsListVM> GetChangesetsAsync(ChangesetsListRequestVM request)
        {
            const int PageSize = 20;

            request = NormalizeListRequest(request);

            var query = _db.Changes
                           .AsNoTracking()
                           .Include(x => x.Author)
                           .Include(x => x.EditedPage)
                           .Include(x => x.EditedMedia)
                           .Include(x => x.EditedRelation)
                           .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var search = request.SearchQuery.ToLower();
                query = query.Where(x => (x.EditedPage != null && x.EditedPage.Title.ToLower().Contains(search))
                                         || x.EditedMedia != null && x.EditedMedia.Title.ToLower().Contains(search));
            }

            if (request.EntityTypes?.Length > 0)
                query = query.Where(x => request.EntityTypes.Contains(x.Type));

            if (request.EntityId != null)
                query = query.Where(x => x.EditedPageId == request.EntityId
                                         || x.EditedMediaId == request.EntityId
                                         || x.EditedRelationId == request.EntityId);

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            if (request.OrderBy == nameof(Changeset.Author))
                query = query.OrderBy(x => x.Author.UserName, request.OrderDescending);
            else
                query = query.OrderBy(x => x.Date, request.OrderDescending);

            var changesets = await query.Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

            var items = changesets.Select(x => new ChangesetTitleVM
                                  {
                                      Id = x.Id,
                                      Date = x.Date,
                                      ChangeType = GetChangeType(x),
                                      Author = x.Author.FirstName + " " + x.Author.LastName,
                                      EntityId = x.EditedPageId ?? x.EditedMediaId ?? x.EditedRelationId ?? Guid.Empty,
                                      EntityType = x.Type,
                                      EntityTitle = GetEntityTitle(x),
                                      EntityThumbnailUrl = GetEntityThumbnailUrl(x),
                                      PageType = GetPageType(x)
                                  })
                                  .ToList();

            return new ChangesetsListVM
            {
                Items = items,
                PageCount = (int) Math.Ceiling((double) totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Returns the details for a changeset.
        /// </summary>
        public async Task<ChangesetDetailsVM> GetChangesetDetailsAsync(Guid id)
        {
            var chg = await _db.Changes
                               .AsNoTracking()
                               .Include(x => x.Author)
                               .GetAsync(x => x.Id == id, "Правка не найдена");

            var renderer = _renderers[chg.Type];
            var prevData = await renderer.RenderValuesAsync(chg.OriginalState);
            var nextData = await renderer.RenderValuesAsync(chg.UpdatedState);

            return new ChangesetDetailsVM
            {
                Id = chg.Id,
                Author = chg.Author.FirstName + " " + chg.Author.LastName,
                Date = chg.Date,
                Type = GetChangeType(chg),
                Changes = GetDiff(prevData, nextData)
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

        /// <summary>
        /// Returns the descriptive title for the changeset. 
        /// </summary>
        private string GetEntityTitle(Changeset chg)
        {
            return chg.EditedPage?.Title
                   ?? chg.EditedMedia?.Title
                   ?? chg.EditedRelation.Type.GetEnumDescription();
        }

        /// <summary>
        /// Returns the thumbnail URL for the changeset.
        /// </summary>
        private string GetEntityThumbnailUrl(Changeset chg)
        {
            return chg.EditedPage?.MainPhoto?.FilePath
                   ?? chg.EditedMedia?.FilePath;
        }

        /// <summary>
        /// Returns the changeset type.
        /// </summary>
        private ChangesetType GetChangeType(Changeset chg)
        {
            var wasNull = string.IsNullOrEmpty(chg.OriginalState);
            var isNull = string.IsNullOrEmpty(chg.UpdatedState);

            if(wasNull)
                return ChangesetType.Created;

            if(isNull)
                return ChangesetType.Removed;

            return ChangesetType.Updated;
        }

        /// <summary>
        /// Returns the page type (if any).
        /// </summary>
        private PageType? GetPageType(Changeset chg)
        {
            return chg.EditedPage?.Type;
        }

        /// <summary>
        /// Returns the list of diffed values.
        /// </summary>
        private IReadOnlyList<ChangeVM> GetDiff(IReadOnlyList<ChangePropertyValue> prevData, IReadOnlyList<ChangePropertyValue> nextData)
        {
            if(prevData.Count != nextData.Count)
                throw new InvalidOperationException("Internal error: rendered changeset values mismatch!");

            var result = new List<ChangeVM>();

            for (var idx = 0; idx < prevData.Count; idx++)
            {
                var prevValue = prevData[idx].Value;
                var nextValue = nextData[idx].Value;

                if (prevValue == nextValue)
                    continue;

                var diff = new HtmlDiff.HtmlDiff(prevValue ?? "", nextValue ?? "").Build();
                result.Add(new ChangeVM
                {
                    Title = prevData[idx].Title,
                    Diff = diff
                });
            }

            return result;
        }

        #endregion
    }
}
