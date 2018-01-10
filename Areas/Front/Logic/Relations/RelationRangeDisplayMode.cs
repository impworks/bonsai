namespace Bonsai.Areas.Front.Logic.Relations
{
    /// <summary>
    /// The way of displaying a date next to a relation.
    /// </summary>
    public enum RelationDurationDisplayMode
    {
        /// <summary>
        /// Display the relation range (spouse).
        /// </summary>
        RelationRange,

        /// <summary>
        /// Display the birth of the related person (child).
        /// </summary>
        Birth,

        /// <summary>
        /// Display the life duration (pet).
        /// </summary>
        Life
    }
}
