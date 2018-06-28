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
        public Guid Id { get; set; }

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
        /// ID of the entity that has been edited.
        /// </summary>
        public Guid EntityId { get; set; }

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
