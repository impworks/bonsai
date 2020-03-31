using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// The log of an entity's modification.
    /// </summary>
    public class Changeset
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Shared value for a group of changesets that were created from a single user action.
        /// </summary>
        public Guid? GroupId { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// User that has authored the edit.
        /// </summary>
        [Required]
        public AppUser Author { get; set; }

        /// <summary>
        /// Type of the changed entity.
        /// </summary>
        public ChangesetEntityType Type { get; set; }

        /// <summary>
        /// Original changeset which has been reverted by the current one.
        /// </summary>
        public Guid? RevertedChangesetId { get; set; }

        /// <summary>
        /// ID of the edited page.
        /// </summary>
        public Guid? EditedPageId { get; set; }

        /// <summary>
        /// Edited page.
        /// </summary>
        public Page EditedPage { get; set; }

        /// <summary>
        /// ID of the edited media.
        /// </summary>
        public Guid? EditedMediaId { get; set; }

        /// <summary>
        /// Edited media.
        /// </summary>
        public Media EditedMedia { get; set; }

        /// <summary>
        /// ID of the edited relation.
        /// </summary>
        public Guid? EditedRelationId { get; set; }

        /// <summary>
        /// Edited relation.
        /// </summary>
        public Relation EditedRelation { get; set; }

        /// <summary>
        /// The original state (before the change).
        /// </summary>
        public string OriginalState { get; set; }

        /// <summary>
        /// The new state (after the change).
        /// </summary>
        public string UpdatedState { get; set; }
    }
}
