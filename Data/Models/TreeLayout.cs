using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Precalculated layout of the family tree.
    /// </summary>
    public class TreeLayout
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Date of the layout's generation.
        /// </summary>
        public DateTimeOffset GenerationDate { get; set; }

        /// <summary>
        /// The rendered layout.
        /// </summary>
        public string LayoutJson { get; set; }
    }
}
