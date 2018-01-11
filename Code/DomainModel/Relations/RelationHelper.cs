using System.Collections.Generic;
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
        };

        /// <summary>
        /// List of allowed relations.
        /// </summary>
        public readonly static RelationBinding[] AllowedRelations = new[]
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
    }
}
