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
        public AppUser Author { get; set; }

        /// <summary>
        /// ID of the entity that has been edited.
        /// </summary>
        public Guid SourceEntityId { get; set; }

        /// <summary>
        /// The diff of the entity's changes.
        /// </summary>
        public string SourceDiff { get; set; }

        /// <summary>
        /// IDs of entities that have been transitively affected by the change.
        /// </summary>
        public string AffectedEntityIds { get; set; }

        /// <summary>
        /// The diff of all other entities' changes.
        /// </summary>
        public string AffectedDiff { get; set; }
    }
}
