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
        public static readonly IReadOnlyDictionary<RelationType, RelationType> ComplementaryRelations = new Dictionary<RelationType, RelationType>
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

            [RelationType.Location] = RelationType.LocationInhabitant,
            [RelationType.LocationInhabitant] = RelationType.Location,
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
            new RelationBinding(PageType.Location, PageType.Person, new[] {RelationType.LocationInhabitant}),
            new RelationBinding(PageType.Person, PageType.Event, new[] {RelationType.Event}),
            new RelationBinding(PageType.Event, PageType.Person, new[] {RelationType.EventVisitor})
        };

        /// <summary>
        /// Returns possible source types for a relation.
        /// </summary>
        public static IReadOnlyList<PageType> SuggestSourcePageTypes(RelationType relType)
        {
            return RelationBindingsMap.Where(x => x.RelationTypes.Contains(relType))
                                      .Select(x => x.SourceType)
                                      .Distinct()
                                      .ToList();
        }

        /// <summary>
        /// Returns possible target types for a relation.
        /// </summary>
        public static IReadOnlyList<PageType> SuggestDestinationPageTypes(RelationType relType)
        {
            return RelationBindingsMap.Where(x => x.RelationTypes.Contains(relType))
                                      .Select(x => x.DestinationType)
                                      .Distinct()
                                      .ToList();
        }

        /// <summary>
        /// Checks if the relation is allowed.
        /// </summary>
        public static bool IsRelationAllowed(PageType source, PageType target, RelationType relation)
        {
            if (relation == RelationType.Other)
                return source == PageType.Other || target == PageType.Other;

            return RelationBindingsMap.Any(x => x.SourceType == source
                                                && x.DestinationType == target
                                                && x.RelationTypes.Contains(relation));
        }

        /// <summary>
        /// Checks if the relation can have an event reference.
        /// </summary>
        public static bool IsRelationEventReferenceAllowed(RelationType relation)
        {
            return relation == RelationType.Spouse;
        }

        /// <summary>
        /// Checks if the relation can have a duration.
        /// </summary>
        public static bool IsRelationDurationAllowed(RelationType relation)
        {
            return relation == RelationType.Spouse
                   || relation == RelationType.Pet
                   || relation == RelationType.Owner
                   || relation == RelationType.StepParent
                   || relation == RelationType.StepChild;
        }
    }
}
