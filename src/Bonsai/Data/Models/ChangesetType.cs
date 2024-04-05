namespace Bonsai.Data.Models
{
    /// <summary>
    /// Kind of the change.
    /// </summary>
    public enum ChangesetType
    {
        /// <summary>
        /// The entity has been created.
        /// </summary>
        Created,

        /// <summary>
        /// The entity has been updated.
        /// </summary>
        Updated,

        /// <summary>
        /// The entity has been removed.
        /// </summary>
        Removed,

        /// <summary>
        /// The previous changeset has been reverted.
        /// </summary>
        Restored
    }
}
