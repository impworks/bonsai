namespace Bonsai.Data.Models
{
    /// <summary>
    /// Types of changesets.
    /// </summary>
    public enum ChangesetEntityType
    {
        /// <summary>
        /// A page has been changed.
        /// </summary>
        Page,

        /// <summary>
        /// A media file has been changed.
        /// </summary>
        Media,

        /// <summary>
        /// A relation has been changed.
        /// </summary>
        Relation
    }
}
