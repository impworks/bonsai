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
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The first entity in the relation.
        /// </summary>
        public virtual Page Subject { get; set; }

        /// <summary>
        /// The second entity in the relation.
        /// </summary>
        public virtual Page Object { get; set; }

        /// <summary>
        /// Name or title of the second entity.
        /// </summary>
        [StringLength(200)]
        public virtual string ObjectTitle { get; set; }

        /// <summary>
        /// Hard-set type of relationship (null for inferred relations).
        /// </summary>
        public virtual RelationType? Type { get; set; }

        /// <summary>
        /// Relation duration.
        /// </summary>
        [StringLength(100)]
        public virtual string Duration { get; set; }

        /// <summary>
        /// Readable title of the relation.
        /// </summary>
        [StringLength(200)]
        public virtual string Title { get; set; }
    }
}
