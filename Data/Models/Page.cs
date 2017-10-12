using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Page describing a single entity.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Page title.
        /// </summary>
        [StringLength(200)]
        public virtual string Title { get; set; }

        /// <summary>
        /// Url-friendly key that is generated from title or entered by the user.
        /// </summary>
        [Key]
        [StringLength(200)]
        public virtual string Key { get; set; }

        /// <summary>
        /// Type of the entity described by this page.
        /// </summary>
        public virtual PageType PageType { get; set; }

        /// <summary>
        /// Free text description of the entity.
        /// </summary>
        [MaxLength]
        public virtual string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current entity.
        /// </summary>
        [MaxLength]
        public virtual string Facts { get; set; }

        /// <summary>
        /// Relations from this page to others.
        /// </summary>
        public virtual ICollection<Relation> Relations { get; set; }

        /// <summary>
        /// Media tags.
        /// </summary>
        public virtual ICollection<MediaTag> MediaTags { get; set; }
    }
}
