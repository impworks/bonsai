using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The presenter for tree elements.
    /// </summary>
    public class TreePresenterService
    {
        #region Constructor

        public TreePresenterService(AppDbContext db)
        {
            _db = db;
        }

        #endregion

        #region Fields

        private readonly AppDbContext _db;

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the entire tree.
        /// </summary>
        public async Task<TreeVM> GetTreeAsync(string rootKey)
        {
            var rootId = await GetRootId(rootKey);
            var context = await RelationContext.LoadContextAsync(_db, new RelationContextOptions { PeopleOnly = true });

            var parents = new HashSet<string>();
            var visited = new HashSet<Guid>();

            var persons = new List<TreePersonVM>();
            var relations = new List<TreeRelationVM>();

            var pending = new Queue<Guid>();
            pending.Enqueue(rootId);

            while (pending.TryDequeue(out var currId))
            {
                if (visited.Contains(currId))
                    continue;

                if (!context.Pages.TryGetValue(currId, out var page))
                    continue;

                visited.Add(currId);
                persons.Add(new TreePersonVM
                {
                    Id = page.Id.ToString(),
                    Name = page.Title,
                    Birth = page.BirthDate?.ShortReadableDate,
                    Death = page.DeathDate?.ShortReadableDate,
                    IsMale = page.Gender ?? true,
                    Photo = page.MainPhotoPath,
                    Url = page.Key,
                    Parents = GetParentRelationshipId(page)
                });

                if (context.Relations.TryGetValue(currId, out var rels))
                {
                    foreach (var rel in rels)
                    {
                        if (rel.Type != RelationType.Child && rel.Type != RelationType.Parent && rel.Type != RelationType.Spouse)
                            continue;

                        pending.Enqueue(rel.DestinationId);
                    }
                }
            }

            return new TreeVM
            {
                Root = rootId.ToString(),
                Persons = persons,
                Relations = relations
            };

            string GetParentRelationshipId(RelationContext.PageExcerpt page)
            {
                if (!context.Relations.TryGetValue(page.Id, out var allRels))
                    return null;

                var rels = allRels.Where(x => x.Type == RelationType.Parent).ToList();
                if (rels.Count == 0)
                    return null;

                var key = rels.Count == 1
                    ? rels[0].DestinationId + ":unknown"
                    : rels.Select(x => x.DestinationId.ToString()).OrderBy(x => x).JoinString(":");

                if (!parents.Contains(key))
                {
                    if (rels.Count == 1)
                    {
                        var fakeId = Guid.NewGuid().ToString();
                        var relPage = context.Pages[rels[0].DestinationId];
                        persons.Add(new TreePersonVM
                        {
                            Id = fakeId,
                            Name = "Неизвестно",
                            IsMale = !(relPage.Gender ?? true)
                        });

                        relations.Add(new TreeRelationVM
                        {
                            Id = key,
                            From = rels[0].DestinationId.ToString(),
                            To = fakeId
                        });
                    }
                    else
                    {
                        relations.Add(new TreeRelationVM
                        {
                            Id = key,
                            From = rels[0].DestinationId.ToString(),
                            To = rels[1].DestinationId.ToString()
                        });
                    }

                    parents.Add(key);
                }

                return key;
            }
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the page's ID by its key.
        /// </summary>
        private async Task<Guid> GetRootId(string key)
        {
            var keyLower = key?.ToLowerInvariant();
            var pageId = await _db.Pages
                                .AsNoTracking()
                                .Where(x => x.Aliases.Any(y => y.Key == keyLower) && x.IsDeleted == false)
                                .Select(x => x.Id)
                                .FirstOrDefaultAsync();

            if(pageId == Guid.Empty)
                throw new KeyNotFoundException();

            return pageId;
        }

        #endregion
    }
}
