using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// The collection of known relation groups.
    /// </summary>
    public static class RelationGroups
    {
        public static RelationGroup[] List =
        {
            new RelationGroup(
                "Родственники",
                RelationType.Parent,
                RelationType.StepParent,
                RelationType.Spouse,
                RelationType.Child,
                RelationType.StepChild,
                null
            ),

            new RelationGroup(
                "Питомцы",
                RelationType.Pet
            ),

            new RelationGroup(
                "Владельцы",
                RelationType.Owner
            ),

            new RelationGroup(
                "Друзья",
                RelationType.Friend
            ),

            new RelationGroup(
                "Коллеги",
                RelationType.Colleague
            ),

            new RelationGroup(
                "Места",
                RelationType.Location
            ),

            new RelationGroup(
                "События",
                RelationType.Event
            ),

            new RelationGroup(
                "Участники",
                RelationType.Participant
            ),
        };
    }
}
