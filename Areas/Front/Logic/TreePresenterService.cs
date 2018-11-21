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
                            AddRelationship(page.Id, rel.DestinationId);
                    }
                }
            }

            return new TreeVM
            {
                Root = rootId.ToString(),
                Persons = persons.Values.OrderBy(x => x.Name).ToList(),
                Relations = relations.Values.OrderBy(x => x.Id).ToList()
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
                        var fakeGender = !(relPage.Gender ?? true);
                        persons.Add(fakeId, new TreePersonVM
                        {
                            Id = fakeId.ToString(),
                            Name = "Неизвестно",
                            IsMale = fakeGender,
                            Photo = GetPhoto(null, fakeGender)
                        });

                        AddRelationship(rels[0].DestinationId, fakeId, relKey);
                    }
                    else
                    {
                        AddRelationship(rels[0].DestinationId, rels[1].DestinationId);
                    }

                    parents.Add(relKey);
                }

                return relKey;
            }

            void AddRelationship(Guid r1, Guid r2, string keyOverride = null)
            {
                var from = r1.ToString();
                var to = r2.ToString();
                if (from.CompareTo(to) >= 1)
                {
                    var tmp = from;
                    from = to;
                    to = tmp;
                }

                var key = keyOverride;
                if (string.IsNullOrEmpty(key))
                    key = from + ":" + to;

                if (relations.ContainsKey(key))
                    return;

                relations.Add(key, new TreeRelationVM
                {
                    Id = key,
                    From = from,
                    To = to
                });
            }
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the photo for a card, depending on the gender, reverting to a default one if unspecified.
        /// </summary>
        string GetPhoto(string actual, bool gender)
        {
            var defaultPhoto = gender
                ? "~/assets/img/unknown-male.png"
                : "~/assets/img/unknown-female.png";

            var photo = StringHelper.Coalesce(actual, defaultPhoto);
            return _url.Content(photo);
        }

        #endregion
    }
}
