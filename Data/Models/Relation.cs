using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Information about a relation between two pages.
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The first entity in the relation.
        /// </summary>
        [Required]
        public Page Source { get; set; }

        /// <summary>
        /// ID of the source page.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// The second entity in the relation.
        /// </summary>
        [Required]
        public Page Destination { get; set; }

        /// <summary>
        /// ID of the second page.
        /// </summary>
        public Guid DestinationId { get; set; }

        /// <summary>
        /// Related event (e.g. wedding).
        /// </summary>
        public Page Event { get; set; }

        /// <summary>
        /// ID of the related event.
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Type of the relation.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// Flag indicating that the relation is automatically generated from an inverse relation specified by an administrator.
        /// </summary>
        public bool IsComplementary { get; set; }

        /// <summary>
        /// Flag indicating that the relation is archived.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Timespan of the relation.
        /// </summary>
        [StringLength(30)]
        public string Duration { get; set; }
    }
}
