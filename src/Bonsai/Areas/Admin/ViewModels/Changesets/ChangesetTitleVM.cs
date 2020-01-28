using System;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Brief information about a changeset.
    /// </summary>
    public class ChangesetTitleVM
    {
        /// <summary>
        /// Changeset ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Type of the change.
        /// </summary>
        public ChangesetType ChangeType { get; set; }

        /// <summary>
        /// Name of the changed entity.
        /// </summary>
        public string EntityTitle { get; set; }

        /// <summary>
        /// URL of the entity's thumbnail.
        /// </summary>
        public string EntityThumbnailUrl { get; set; }

        /// <summary>
        /// Type of the changed entity.
        /// </summary>
        public ChangesetEntityType EntityType { get; set; }

        /// <summary>
        /// ID of the entity that has been edited.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Author of the change.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Type of the page (if the changeset is page-related).
        /// </summary>
        public PageType? PageType { get; set; }

        /// <summary>
        /// Flag indicating that this changeset can be reverted.
        /// </summary>
        public bool CanRevert { get; set; }
    }
}
