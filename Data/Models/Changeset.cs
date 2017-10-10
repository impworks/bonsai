using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// The log of a page's edit and its consequences on other pages.
    /// </summary>
    public class Changeset
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Edit date.
        /// </summary>
        public virtual DateTimeOffset Date { get; set; }

        /// <summary>
        /// User that has authored the edit.
        /// </summary>
        public virtual AppUser Author { get; set; }

        /// <summary>
        /// ID of the entity that has been edited.
        /// </summary>
        public virtual Guid SourceEntityId { get; set; }

        /// <summary>
        /// The diff of the entity's changes.
        /// </summary>
        [MaxLength]
        public virtual string SourceDiff { get; set; }

        /// <summary>
        /// IDs of entities that have been transitively affected by the change.
        /// </summary>
        [MaxLength]
        public virtual string AffectedEntityIds { get; set; }

        /// <summary>
        /// The diff of all other entities' changes.
        /// </summary>
        [MaxLength]
        public virtual string AffectedDiff { get; set; }
    }
}
