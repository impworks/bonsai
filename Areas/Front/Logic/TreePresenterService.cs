using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The presenter for tree elements.
    /// </summary>
    public class TreePresenterService
    {
        #region Constructor

        public TreePresenterService(AppDbContext db, IUrlHelper url)
        {
            _db = db;
            _url = url;
        }

        #endregion

        #region Fields

        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the entire tree.
        /// </summary>
        public async Task<TreeVM> GetTreeAsync(Guid rootId)
        {
            var context = await RelationContext.LoadContextAsync(_db, new RelationContextOptions { PeopleOnly = true });

            var parents = new HashSet<string>();

            var persons = new Dictionary<Guid, TreePersonVM>();
            var relations = new Dictionary<string, TreeRelationVM>();

            var pending = new Queue<Guid>();
            pending.Enqueue(rootId);

            while (pending.TryDequeue(out var currId))
            {
                if (persons.ContainsKey(currId))
                    continue;

                if (!context.Pages.TryGetValue(currId, out var page))
                    continue;

                persons.Add(
                    page.Id,
                    new TreePersonVM
                    {
                        Id = page.Id.ToString(),
                        Name = page.Title,
                        Birth = page.BirthDate?.ShortReadableDate,
                        Death = page.DeathDate?.ShortReadableDate,
                        IsMale = page.Gender ?? true,
                        Photo = GetPhoto(page.MainPhotoPath, page.Gender ?? true),
                        Url = _url.Action("Description", "Page", new { area = "Front", key = page.Key }),
                        Parents = GetParentRelationshipId(page)
                    }
                );

                if (context.Relations.TryGetValue(currId, out var rels))
                {
                    foreach (var rel in rels)
                    {
                        if (rel.Type != RelationType.Child && rel.Type != RelationType.Parent && rel.Type != RelationType.Spouse)
                            continue;

                        pending.Enqueue(rel.DestinationId);

                        if(rel.Type == RelationType.Spouse)
                            AddRelationship(page.Id.ToString(), rel.DestinationId.ToString());
                    }
                }
            }

            return new TreeVM
            {
                Root = rootId.ToString(),
                Persons = persons.Values.ToList(),
                Relations = relations.Values.ToList()
            };

            string GetParentRelationshipId(RelationContext.PageExcerpt page)
            {
                if (!context.Relations.TryGetValue(page.Id, out var allRels))
                    return null;

                var rels = allRels.Where(x => x.Type == RelationType.Parent).ToList();
                if (rels.Count == 0)
                    return null;

                var relKey = rels.Count == 1
                    ? rels[0].DestinationId + ":unknown"
                    : rels.Select(x => x.DestinationId.ToString()).OrderBy(x => x).JoinString(":");

                if (!parents.Contains(relKey))
                {
                    if (rels.Count == 1)
                    {
                        var fakeId = Guid.NewGuid();
                        var relPage = context.Pages[rels[0].DestinationId];
                        persons.Add(fakeId, new TreePersonVM
                        {
                            Id = fakeId.ToString(),
                            Name = "Неизвестно",
                            IsMale = !(relPage.Gender ?? true)
                        });

                        AddRelationship(rels[0].DestinationId.ToString(), fakeId.ToString(), relKey);
                    }
                    else
                    {
                        AddRelationship(rels[0].DestinationId.ToString(), rels[1].DestinationId.ToString());
                    }

                    parents.Add(relKey);
                }

                return relKey;
            }

            void AddRelationship(string from, string to, string keyOverride = null)
            {
                var key = keyOverride;
                if (string.IsNullOrEmpty(key))
                {
                    key = string.Compare(from, to) > 0
                        ? to + ":" + from
                        : from + ":" + to;
                }

                if (relations.ContainsKey(key))
                    return;

                relations.Add(key, new TreeRelationVM
                {
                    Id = key,
                    From = from,
                    To = to
                });
            }

            string GetPhoto(string actual, bool gender)
            {
                var defaultPath = gender
                    ? "~/assets/img/unknown-male.png"
                    : "~/assets/img/unknown-female.png";

                return _url.Content(StringHelper.Coalesce(actual, defaultPath));
            }
        }

        #endregion
    }
}
