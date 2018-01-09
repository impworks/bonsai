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
        /// Type of the relation.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// Flag indicating that the relation is automatically inferred.
        /// Inferred relations are flushed and re-generated each time an admin updates the relation list.
        /// </summary>
        public bool IsInferred { get; set; }

        /// <summary>
        /// Timespan of the relation.
        /// </summary>
        [StringLength(30)]
        public string Duration { get; set; }
    }
}
