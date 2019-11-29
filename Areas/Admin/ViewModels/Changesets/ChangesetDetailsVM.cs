using System;
using System.Collections.Generic;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Base information about a changeset.
    /// </summary>
    public class ChangesetDetailsVM
    {
        /// <summary>
        /// ID of the changeset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// User that has authored the edit.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Type of the change.
        /// </summary>
        public ChangesetType ChangeType { get; set; }

        /// <summary>
        /// Type of the changed entity.
        /// </summary>
        public ChangesetEntityType EntityType { get; set; }

        /// <summary>
        /// URL of the thumbnail (for media changesets).
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Changed items.
        /// </summary>
        public IReadOnlyList<ChangeVM> Changes { get; set; }

        /// <summary>
        /// Flag indicating that this changeset can be reverted.
        /// </summary>
        public bool CanRevert { get; set; }
    }
}
