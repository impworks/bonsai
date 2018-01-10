using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.Tools;
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

        #region Relation definitions

        /// <summary>
        /// Relations for parents/siblings group.
        /// </summary>
        public static RelationDefinition[] ParentRelations =
        {
            new RelationDefinition("Parent:m", "Отец"),
            new RelationDefinition("Parent:f", "Мать"),
            new RelationDefinition("Parent Child:m", "Брат", "Братья"),
            new RelationDefinition("Parent Child:f", "Сестра", "Сестры"),
            new RelationDefinition("Parent Parent:m", "Дедушка", "Дедушки"),
            new RelationDefinition("Parent Parent:f", "Бабушка", "Бабушки")
        };

        /// <summary>
        /// Relations for a spouse-based group.
        /// </summary>
        public static RelationDefinition[] SpouseDefinitions =
        {
            new RelationDefinition("Spouse:m", "Муж", null, RelationDurationDisplayMode.RelationRange), 
            new RelationDefinition("Spouse:f", "Жена", null, RelationDurationDisplayMode.RelationRange),
            new RelationDefinition("Spouse Child+Child", "Сын|Дочь|Ребенок", "Дети", RelationDurationDisplayMode.Birth),
            new RelationDefinition("Spouse:m Parent:m", "Свекр"),
            new RelationDefinition("Spouse:m Parent:f", "Свекровь"),
            new RelationDefinition("Spouse:f Parent:m", "Тесть"),
            new RelationDefinition("Spouse:f Parent:f", "Теща"),
            new RelationDefinition("Spouse:m Parent Child:m", "Деверь", "Девери"),
            new RelationDefinition("Spouse:m Parent Child:f", "Золовка", "Золовки"),
            new RelationDefinition("Spouse:f Parent Child:m", "Шурин", "Шурины"),
            new RelationDefinition("Spouse:f Parent Child:f", "Свояченица", "Свояченицы")
        };

        /// <summary>
        /// Other relations for 
        /// </summary>
        public static RelationDefinition[][] OtherRelationGroups =
        {
            new []
            {
                new RelationDefinition("Child-Spouse Child", "Сын|Дочь|Ребенок", "Дети", RelationDurationDisplayMode.Birth),
                new RelationDefinition("Child Child", "Внук|Внучка|Внук", "Дети", RelationDurationDisplayMode.Birth),
            },
            new[]
            {
                new RelationDefinition("Friend", "Друг", "Друзья"),
                new RelationDefinition("Colleague", "Коллега", "Коллеги"),
            },

            new[]
            {
                new RelationDefinition("Owner", "Владелец", "Владельцы", RelationDurationDisplayMode.RelationRange),
            },

            new[]
            {
                new RelationDefinition("Pet", "Питомец", "Питомцы"),
            },
        };

        #endregion

        #region Public methods

        /// <summary>
        /// Returns the list of all inferred relation groups for the page.
        /// </summary>
        public async Task<IReadOnlyList<RelationGroupVM>> GetRelationsForPage(Guid pageId)
        {
            var ctx = await LoadRelationsContext().ConfigureAwait(false);

            return GetParentsGroups(ctx, pageId)
                   .Concat(GetSpouseGroups(ctx, pageId))
                   .Concat(GetOtherGroups(ctx, pageId))
                   .ToList();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Returns the relation groups with parents and siblings.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetParentsGroups(RelationContext ctx, Guid pageId)
        {
            var page = ctx.Pages[pageId];
            if (page.Type != PageType.Person && page.Type != PageType.Pet)
                yield break;

            var ids = new[] {pageId};
            var relations = ParentRelations.Select(x => GetRelationVM(ctx, x, ids))
                                           .Where(x => x != null)
                                           .ToList();

            if (relations.Any())
                yield return new RelationGroupVM {Relations = relations};
        }

        /// <summary>
        /// Returns the groups for each spouse-based family.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetSpouseGroups(RelationContext ctx, Guid pageId)
        {
            var page = ctx.Pages[pageId];
            if (page.Type != PageType.Person && page.Type != PageType.Pet)
                yield break;

            var spouses = ctx.Relations[pageId].Where(x => x.Type == RelationType.Spouse);
            foreach (var spouse in spouses)
            {
                var ids = new[] {pageId, spouse.DestinationId};
                var relations = SpouseDefinitions.Select(x => GetRelationVM(ctx, x, ids))
                                                 .Where(x => x != null)
                                                 .ToList();

                if (relations.Any())
                    yield return new RelationGroupVM {Relations = relations};
            }
        }

        /// <summary>
        /// Returns the secondary relation groups.
        /// </summary>
        private IEnumerable<RelationGroupVM> GetOtherGroups(RelationContext ctx, Guid pageId)
        {
            foreach (var groupDef in OtherRelationGroups)
            {
                var ids = new[] {pageId};
                var relations = groupDef.Select(x => GetRelationVM(ctx, x, ids))
                                        .Where(x => x != null)
                                        .ToList();

                if (relations.Any())
                    yield return new RelationGroupVM {Relations = relations};
            }
        }

        /// <summary>
        /// Loads basic information about all pages.
        /// </summary>
        private async Task<RelationContext> LoadRelationsContext()
        {
            var pages = await _db.Pages
                            .Select(x => new PageExcerpt
                            {
                                Id = x.Id,
                                Key = x.Key, 
                                Title = x.Title,
                                Type = x.PageType,
                                Gender = x.Gender
                            })
                            .ToDictionaryAsync(x => x.Id, x => x)
                            .ConfigureAwait(false);

            var relations = await _db.Relations
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

            return new RelationContext {Pages = pages, Relations = relations};
        }

        /// <summary>
        /// Returns a relation for all pages matching the definition.
        /// </summary>
        private RelationVM GetRelationVM(RelationContext ctx, RelationDefinition def, params Guid[] guids)
        {
            IEnumerable<RelationTarget> Step(RelationTarget elem, RelationPathSegment segment, Guid? guidFilter)
            {
                return from rel in ctx.Relations[elem.Page.Id]
                       where rel.Type == segment.Type
                       where guidFilter == null || rel.DestinationId == guidFilter
                       let page = ctx.Pages[rel.DestinationId]
                       where segment.Gender == null || segment.Gender == page.Gender
                       where !elem.VisitedPages.Contains(page)
                       select new RelationTarget(page, rel, elem.VisitedPages.Append(page));
            }

            IEnumerable<RelationTarget> ProcessPath(RelationPath path)
            {
                var root = ctx.Pages[guids[0]];
                var currents = new List<RelationTarget> {new RelationTarget(root, null, new SinglyLinkedList<PageExcerpt>(root))};

                for (var depth = 0; depth < path.Segments.Count; depth++)
                {
                    if (currents.Count == 0)
                        break;

                    var segment = path.Segments[depth];
                    var guidFilter = !path.IsExcluded && (depth + 1) < guids.Length ? guids[depth + 1] : (Guid?)null;
                    currents = currents.Select(x => Step(x, segment, guidFilter))
                                       .SelectMany(x => x)
                                       .ToList();
                }

                return currents;
            }
            
            RelatedPageVM Convert(RelationTarget elem)
            {
                return new RelatedPageVM
                {
                    Title = elem.Page.Title,
                    Key = elem.Page.Key,
                    RelationRange = FuzzyRange.Parse(elem.Relation.Duration)
                };
            }

            var posPaths = def.Paths.Where(x => !x.IsExcluded).Select(ProcessPath);
            var negPaths = def.Paths.Where(x => x.IsExcluded).Select(ProcessPath);

            var results = posPaths.Aggregate((a, b) => a.Concat(b))
                                  .Except(negPaths.SelectMany(x => x))
                                  .ToList();

            return results.Any()
                ? new RelationVM
                {
                    Title = def.GetName(results.Count, results[0].Page.Gender),
                    Pages = results.Select(Convert).ToList()
                }
                : null;
        }

        #endregion

        #region Data classes

        /// <summary>
        /// Basic information about a page.
        /// </summary>
        private class PageExcerpt: IEquatable<PageExcerpt>
        {
            public Guid Id;
            public string Title;
            public string Key;
            public PageType Type;
            public bool? Gender;

            #region Equality members (auto-generated)

            public bool Equals(PageExcerpt other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || Id.Equals(other.Id));
            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((PageExcerpt) obj));
            public override int GetHashCode() => Id.GetHashCode();

            #endregion
        }

        /// <summary>
        /// Basic information about a relation between two pages.
        /// </summary>
        private class RelationExcerpt
        {
            public Guid SourceId;
            public Guid DestinationId;
            public RelationType Type;
            public string Duration;
        }

        private class RelationTarget: IEquatable<RelationTarget>
        {
            public readonly PageExcerpt Page;
            public readonly RelationExcerpt Relation;
            public readonly SinglyLinkedList<PageExcerpt> VisitedPages;

            public RelationTarget(PageExcerpt page, RelationExcerpt relation, SinglyLinkedList<PageExcerpt> visitedPages)
            {
                Page = page;
                Relation = relation;
                VisitedPages = visitedPages;
            }

            #region Equality members (auto-generated)

            public bool Equals(RelationTarget other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || Page.Equals(other.Page));
            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((RelationTarget) obj));
            public override int GetHashCode() => Page.GetHashCode();

            #endregion
        }

        private class RelationContext
        {
            public IReadOnlyDictionary<Guid, PageExcerpt> Pages;
            public IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> Relations;
        }

        #endregion
    }
}
