namespace Bonsai.Data.Models
{
    /// <summary>
    /// Kinds of entity relationships.
    /// </summary>
    public enum RelationType
    {
        Parent,
        Child,
        Spouse,

        StepParent,
        StepChild,

        Friend,
        Colleague,

        Owner,
        Pet,

        Location,
        LocationVisitor,

        Event,
        EventVisitor,

        Other
    }
}
