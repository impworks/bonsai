using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Impworks.Utils.Strings;

namespace Bonsai.Areas.Front.Logic.Relations;

/// <summary>
/// The service for calculating relations between pages.
/// </summary>
public partial class RelationsPresenterService
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
    [
        new RelationDefinition("Parent:m", Texts.Relations_ParentM), // отец
        new RelationDefinition("Parent:f", Texts.Relations_ParentF), // мать
        new RelationDefinition("Parent-Parent:m-Parent:f", Texts.Relations_Parent, Texts.Relations_Parent_Mult), // родители
        new RelationDefinition("Parent Child:m", Texts.Relations_ParentChildM, Texts.Relations_ParentChildM_Mult), // брат
        new RelationDefinition("Parent Child:f", Texts.Relations_ParentChildF, Texts.Relations_ParentChildF_Mult), // сестра
        new RelationDefinition("Parent Parent:m", Texts.Relations_ParentParentM, Texts.Relations_ParentParentM_Mult), // бабушка
        new RelationDefinition("Parent Parent:f", Texts.Relations_ParentParentF, Texts.Relations_ParentParentF_Mult) // дедушка
    ];

    /// <summary>
    /// Relations for a spouse-based group.
    /// </summary>
    public static RelationDefinition[] SpouseDefinitions =
    [
        new RelationDefinition("!Spouse:m", Texts.Relations_SpouseM, null, RelationDurationDisplayMode.RelationRange), // муж
        new RelationDefinition("!Spouse:f", Texts.Relations_SpouseF, null, RelationDurationDisplayMode.RelationRange), // жена
        new RelationDefinition("!Spouse Child+Child", Texts.Relations_SpouseChild, Texts.Relations_SpouseChild_Mult, RelationDurationDisplayMode.Birth), // родной ребенок с супругом
        new RelationDefinition("!Spouse:m Parent:m", Texts.Relations_SpouseMParentM), // свекр
        new RelationDefinition("!Spouse:m Parent:f", Texts.Relations_SpouseMParentF), // свекровь
        new RelationDefinition("!Spouse:f Parent:m", Texts.Relations_SpouseFParentM), // тесть
        new RelationDefinition("!Spouse:f Parent:f", Texts.Relations_SpouseFParentF), // теща
        new RelationDefinition("!Spouse:m Parent Child:m", Texts.Relations_SpouseMParentChildM, Texts.Relations_SpouseMParentChildM_Mult), // деверь
        new RelationDefinition("!Spouse:m Parent Child:f", Texts.Relations_SpouseMParentChildF, Texts.Relations_SpouseMParentChildF_Mult), // золовка
        new RelationDefinition("!Spouse:f Parent Child:m", Texts.Relations_SpouseFParentChildM, Texts.Relations_SpouseFParentChildM_Mult), // шурин
        new RelationDefinition("!Spouse:f Parent Child:f", Texts.Relations_SpouseFParentChildF, Texts.Relations_SpouseFParentChildF_Mult) // свояченица
    ];

    /// <summary>
    /// Other relations for family members.
    /// </summary>
    public static RelationDefinition[] OtherRelativeRelations =
    [
        new RelationDefinition("Child-Spouse Child", Texts.Relations_SpouseOtherChild, Texts.Relations_SpouseOtherChild_Mult, RelationDurationDisplayMode.Birth), // ребенок не от супруга
        new RelationDefinition("Child Child", Texts.Relations_ChildChild, Texts.Relations_ChildChild_Mult, RelationDurationDisplayMode.Birth), // внук
        new RelationDefinition("Child:f Spouse:m", Texts.Relations_ChildFSpouseM, Texts.Relations_ChildFSpouseM_Mult), // зять
        new RelationDefinition("Child:m Spouse:f", Texts.Relations_ChildMSpouseF, Texts.Relations_ChildMSpouseF_Mult), // невестка
        new RelationDefinition("Pet", Texts.Relations_Pet, Texts.Relations_Pet_Mult) // питомец
    ];

    /// <summary>
    /// Relations for other people.
    /// </summary>
    private static RelationDefinition[] NonRelativeRelations =
    [
        new RelationDefinition("Friend", Texts.Relations_Friend, Texts.Relations_Friend_Mult), // друг
        new RelationDefinition("Colleague", Texts.Relations_Colleague, Texts.Relations_Colleague_Mult), // коллега
        new RelationDefinition("Owner", Texts.Relations_Owner, Texts.Relations_Owner_Mult, RelationDurationDisplayMode.RelationRange), // владелец
        new RelationDefinition("EventVisitor", Texts.Relations_EventVisitor, Texts.Relations_EventVisitor_Mult), // участник
        new RelationDefinition("LocationInhabitant", Texts.Relations_LocationInhabitant, Texts.Relations_LocationInhabitant_Mult) // житель
    ];

    /// <summary>
    /// Relations for non-human pages.
    /// </summary>
    private static RelationDefinition[] NonHumanRelations =
    [
        new RelationDefinition("Location", Texts.Relations_Location, Texts.Relations_Location_Mult), // место
        new RelationDefinition("Event", Texts.Relations_Event, Texts.Relations_Event_Mult) // событие
    ];

    #endregion

    #region Public methods

    /// <summary>
    /// Returns the list of all inferred relation groups for the page.
    /// </summary>
    public async Task<IReadOnlyList<RelationCategoryVM>> GetRelationsForPage(Guid pageId)
    {
        var ctx = await RelationContext.LoadContextAsync(_db);

        var cats = new []
        {
            new RelationCategoryVM
            {
                Title = Texts.Relations_Group_Relatives,
                IsMain = true,
                Groups = GetGroups(ctx, pageId, ParentRelations)
                         .Concat(GetSpouseGroups(ctx, pageId))
                         .Concat(GetGroups(ctx, pageId, OtherRelativeRelations))
                         .ToList()
            },
            new RelationCategoryVM
            {
                Title = Texts.Relations_Group_People,
                Groups = GetGroups(ctx, pageId, NonRelativeRelations).ToList(),
            },
            new RelationCategoryVM
            {
                Title = Texts.Relations_Group_Pages,
                Groups = GetGroups(ctx, pageId, NonHumanRelations).ToList(),
            }
        };

        return cats.Where(x => x.Groups.Any()).ToList();
    }

    #endregion

    #region Private helpers

    /// <summary>
    /// Returns the relation groups with parents and siblings.
    /// </summary>
    private IEnumerable<RelationGroupVM> GetGroups(RelationContext ctx, Guid pageId, RelationDefinition[] defs)
    {
        var ids = new[] {pageId};
        var relations = defs.Select(x => GetRelationVM(ctx, x, ids))
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
        if (!ctx.Pages.TryGetValue(pageId, out var page))
            yield break;

        if (page.Type != PageType.Person && page.Type != PageType.Pet)
            yield break;

        if (!ctx.Relations.ContainsKey(pageId))
            yield break;

        var spouses = ctx.Relations[pageId]
                         .Where(x => x.Type == RelationType.Spouse)
                         .OrderBy(x => x.Duration);

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
    /// Returns a relation for all pages matching the definition.
    /// </summary>
    private RelationVM GetRelationVM(RelationContext ctx, RelationDefinition def, params Guid[] guids)
    {
        // Performs one step from the current page along the relation path and returns matching pages
        IEnumerable<RelationTarget> Step(RelationTarget elem, RelationPathSegment segment, Guid? guidFilter)
        {
            if(!ctx.Relations.TryGetValue(elem.Page.Id, out var rels))
                return Enumerable.Empty<RelationTarget>();

            return from rel in rels
                   where rel.Type == segment.Type
                   where guidFilter == null || rel.DestinationId == guidFilter
                   let page = ctx.Pages[rel.DestinationId]
                   where segment.Gender == null || segment.Gender == page.Gender
                   where !elem.VisitedPages.Contains(page)
                   select new RelationTarget(page, rel, elem.VisitedPages.Append(page));
        }
            
        // Finds pages matching the entire path from current page
        IEnumerable<RelationTarget> GetMatchingPages(RelationPath path)
        {
            if (!ctx.Pages.TryGetValue(guids[0], out var root))
                return Array.Empty<RelationTarget>();

            var currents = new List<RelationTarget> {new RelationTarget(root, null, new SinglyLinkedList<RelationContext.PageExcerpt>(root))};

            for (var depth = 0; depth < path.Segments.Count; depth++)
            {
                if (currents.Count == 0)
                    break;

                var segment = path.Segments[depth];
                var guidFilter = path.IsBound && (depth + 1) < guids.Length
                    ? guids[depth + 1]
                    : (Guid?) null;

                currents = currents.Select(x => Step(x, segment, guidFilter))
                                   .SelectMany(x => x)
                                   .ToList();
            }

            return currents;
        }
            
        // Gets the range to display alongside the relation
        FuzzyRange? GetRange(RelationTarget elem)
        {
            if (def.DurationDisplayMode == RelationDurationDisplayMode.RelationRange)
                return elem.Relation.Duration;

            if (def.DurationDisplayMode == RelationDurationDisplayMode.Birth)
                if (elem.Page.BirthDate != null)
                    return new FuzzyRange(elem.Page.BirthDate, null);

            if (def.DurationDisplayMode == RelationDurationDisplayMode.Life)
                if(elem.Page.BirthDate != null || elem.Page.DeathDate != null)
                    return new FuzzyRange(elem.Page.BirthDate, elem.Page.DeathDate);

            return null;
        }

        PageTitleVM GetEventPageTitle(Guid? eventId)
        {
            if (eventId == null)
                return null;

            var page = ctx.Pages[eventId.Value];
            return new PageTitleVM
            {
                Title = page.Title,
                Key = page.Key
            };
        }

        var posPaths = def.Paths.Where(x => !x.IsExcluded);
        var negPaths = def.Paths.Where(x => x.IsExcluded);

        // A+B-C means: all pages matching both paths A & B, but not matching path C
        var results = posPaths.Select(GetMatchingPages)
                              .Aggregate((a, b) => a.Intersect(b))
                              .Except(negPaths.Select(GetMatchingPages)
                                              .SelectMany(x => x))
                              .ToList();
             
        if(!results.Any())
            return null;

        return new RelationVM
        {
            Title = def.GetName(results.Count, results[0].Page.Gender),
            Pages = results.OrderBy(x => x.Page.BirthDate)
                           .Select(elem => new RelatedPageVM
                           {
                               Title = StringHelper.Coalesce(elem.Page.ShortName, elem.Page.Title),
                               Key = elem.Page.Key,
                               Duration = GetRange(elem),
                               RelationEvent = GetEventPageTitle(elem.Relation.EventId)
                           })
                           .ToList()
        };
    }

    #endregion
}