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
        public Page Subject { get; set; }

        /// <summary>
        /// The second entity in the relation.
        /// </summary>
        [Required]
        public Page Object { get; set; }

        /// <summary>
        /// Direct type of relationship (for explicitly specified relations).
        /// </summary>
        public RelationType? Type { get; set; }

        /// <summary>
        /// Relation path as comma-separated relatiom types (for inferred relations).
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Timespan of the relation.
        /// </summary>
        [StringLength(30)]
        public string Duration { get; set; }

        /// <summary>
        /// Readable title of the relation.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
    }
}
