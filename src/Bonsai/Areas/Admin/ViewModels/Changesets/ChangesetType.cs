using System.ComponentModel;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Kind of the change.
    /// </summary>
    public enum ChangesetType
    {
        /// <summary>
        /// The entity has been created.
        /// </summary>
        [Description("Создано")]
        Created,

        /// <summary>
        /// The entity has been updated.
        /// </summary>
        [Description("Изменено")]
        Updated,

        /// <summary>
        /// The entity has been removed.
        /// </summary>
        [Description("Удалено")]
        Removed,

        /// <summary>
        /// The previous changeset has been reverted.
        /// </summary>
        [Description("Восстановлено")]
        Restored
    }
}
