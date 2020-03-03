using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// General application configuration.
    /// </summary>
    public class DynamicConfigWrapper
    {
        /// <summary>
        /// Surrogate key.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Serialized configuration.
        /// </summary>
        public string Value { get; set; }
    }
}
