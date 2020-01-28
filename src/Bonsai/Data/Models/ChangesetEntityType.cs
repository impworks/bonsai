using System.ComponentModel;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Types of changesets.
    /// </summary>
    public enum ChangesetEntityType
    {
        /// <summary>
        /// A page has been changed
        /// </summary>
        [Description("Страница")]
        Page,

        /// <summary>
        /// A media file has been changed
        /// </summary>
        [Description("Медиа")]
        Media,

        /// <summary>
        /// A relation has been changed
        /// </summary>
        [Description("Связь")]
        Relation
    }
}
