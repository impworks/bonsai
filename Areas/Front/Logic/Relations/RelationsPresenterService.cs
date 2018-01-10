using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// The service for calculating relations between pages.
    /// </summary>
    public class RelationsPresenterService
    {
        public RelationsPresenterService(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        #region Public methods

        /// <summary>
        /// Returns the list of all inferred relation groups for the page.
        /// </summary>
        public async Task<IReadOnlyList<RelationGroupVM>> GetRelationsForPage(Guid pageId)
        {
            var pages = await LoadPageExcerptsAsync().ConfigureAwait(false);
            var rels = await LoadRelationExcerptsAsync().ConfigureAwait(false);

            return GetParentsGroups(pageId, pages, rels)
                   .Concat(GetSpouseGroups(pageId, pages, rels))
                   .Concat(GetOtherGroups(pageId, pages, rels))
                   .ToList();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the relation groups with parents and siblings.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetParentsGroups(Guid pageId, IReadOnlyList<PageExcerpt> pages, IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> rels)
        {
            yield break;
        }

        /// <summary>
        /// Returns the groups for each spouse-based family.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetSpouseGroups(Guid pageId, IReadOnlyList<PageExcerpt> pages, IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> rels)
        {
            yield break;
        }

        /// <summary>
        /// Returns the non-human relation groups.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetOtherGroups(Guid pageId, IReadOnlyList<PageExcerpt> pages, IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> rels)
        {
            yield break;
        }

        /// <summary>
        /// Loads basic information about all pages.
        /// </summary>
        private async Task<IReadOnlyList<PageExcerpt>> LoadPageExcerptsAsync()
        {
            return await _db.Pages
                            .Select(x => new PageExcerpt
                            {
                                Id = x.Id,
                                Key = x.Key, 
                                Title = x.Title,
                                Gender = x.Gender
                            })
                            .ToListAsync()
                            .ConfigureAwait(false);
        }

        /// <summary>
        /// Loads basic information about all relations.
        /// </summary>
        private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>>> LoadRelationExcerptsAsync()
        {
            return await _db.Relations
                            .Select(x => new RelationExcerpt
                            {
                                SourceId = x.SourceId,
                                DestinationId = x.DestinationId,
                                Duration = x.Duration,
                                Type = x.Type
                            })
                            .GroupBy(x => x.SourceId)
                            .ToDictionaryAsync(x => x.Key, x => (IReadOnlyList<RelationExcerpt>) x.ToList())
                            .ConfigureAwait(false);
        }

        #endregion

        #region Data classes

        /// <summary>
        /// Basic information about a page.
        /// </summary>
        private class PageExcerpt
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Key { get; set; }
            public bool? Gender { get; set; }
        }

        /// <summary>
        /// Basic information about a relation between two pages.
        /// </summary>
        private class RelationExcerpt
        {
            public Guid SourceId { get; set; }
            public Guid DestinationId { get; set; }
            public RelationType Type { get; set; }
            public string Duration { get; set; }
        }

        #endregion
    }
}
