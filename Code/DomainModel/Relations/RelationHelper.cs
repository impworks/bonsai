using System.Collections.Generic;
using System.Linq;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Relations
{
    /// <summary>
    /// A collection of helpful utilities for working with relations.
    /// </summary>
    public static class RelationHelper
    {
        /// <summary>
        /// Complementary relation bindings.
        /// </summary>
        public static IReadOnlyDictionary<RelationType, RelationType> ComplementaryRelations = new Dictionary<RelationType, RelationType>
        {
            [RelationType.Parent] = RelationType.Child,
            [RelationType.Child] = RelationType.Parent,
            [RelationType.StepParent] = RelationType.StepChild,
            [RelationType.StepChild] = RelationType.StepParent,

            [RelationType.Pet] = RelationType.Owner,
            [RelationType.Owner] = RelationType.Pet,

            [RelationType.Spouse] = RelationType.Spouse,
            [RelationType.Friend] = RelationType.Friend,
            [RelationType.Colleague] = RelationType.Colleague,

            [RelationType.Location] = RelationType.LocationVisitor,
            [RelationType.LocationVisitor] = RelationType.Location,
            [RelationType.Event] = RelationType.EventVisitor,
            [RelationType.EventVisitor] = RelationType.Event,

            [RelationType.Other] = RelationType.Other
        };

        /// <summary>
        /// List of allowed relations.
        /// </summary>
        public static readonly RelationBinding[] RelationBindingsMap =
        {
            new RelationBinding(PageType.Person, PageType.Person, new[]
            {
                RelationType.Parent,
                RelationType.Child,
                RelationType.Spouse,
                RelationType.Friend,
                RelationType.Colleague,
                RelationType.StepParent,
                RelationType.StepChild
            }),
            new RelationBinding(PageType.Person, PageType.Pet, new[] {RelationType.Pet}),
            new RelationBinding(PageType.Pet, PageType.Person, new[] {RelationType.Owner}),
            new RelationBinding(PageType.Person, PageType.Location, new[] {RelationType.Location}),
            new RelationBinding(PageType.Location, PageType.Person, new[] {RelationType.LocationVisitor}),
            new RelationBinding(PageType.Person, PageType.Event, new[] {RelationType.Event}),
            new RelationBinding(PageType.Event, PageType.Person, new[] {RelationType.EventVisitor})
        };

        /// <summary>
        /// Suggests available relation types for one or two pages.
        /// </summary>
        public static IReadOnlyList<RelationType> SuggestRelationTypes(PageType sourceType, PageType? targetType = null)
        {
            var types = RelationBindingsMap.Where(x => x.SourceType == sourceType
                                                      && (targetType == null || targetType == x.TargetType))
                                          .SelectMany(x => x.RelationTypes)
                                          .Distinct()
                                          .ToList();

            if(sourceType == PageType.Other || targetType == PageType.Other)
                types.Add(RelationType.Other);

            return types;
        }

        /// <summary>
        /// Suggests available page types for a page and a relation.
        /// </summary>
        public static IReadOnlyList<PageType> SuggestPageTypes(PageType sourceType, RelationType? relType = null)
        {
            var types = RelationBindingsMap.Where(x => x.SourceType == sourceType
                                                       && (relType == null || x.RelationTypes.Contains(relType.Value)))
                                           .Select(x => x.TargetType)
                                           .Distinct()
                                           .ToList();

            if(sourceType == PageType.Other || relType == RelationType.Other)
                types.Add(PageType.Other);

            return types;
        }

        /// <summary>
        /// Checks if the relation is allowed.
        /// </summary>
        public static bool IsRelationAllowed(PageType source, PageType target, RelationType relation)
        {
            if (relation == RelationType.Other)
                return source == PageType.Other || target == PageType.Other;

            return RelationBindingsMap.Any(x => x.SourceType == source
                                                && x.TargetType == target
                                                && x.RelationTypes.Contains(relation));
        }

        /// <summary>
        /// Checks if the relation can have an event reference.
        /// </summary>
        public static bool IsRelationEventReferenceAllowed(RelationType relation)
        {
            return relation == RelationType.Spouse;
        }
    }
}
