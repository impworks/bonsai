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
        public Guid Id { get; set; }

        /// <summary>
        /// Page title.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Page key (title, url-encoded).
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Key { get; set; }

        /// <summary>
        /// Type of the entity described by this page.
        /// </summary>
        public PageType PageType { get; set; }

        /// <summary>
        /// Free text description of the entity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current entity.
        /// </summary>
        public string Facts { get; set; }


        /// <summary>
        /// Relations from this page to others.
        /// </summary>
        public ICollection<Relation> Relations { get; set; }

        /// <summary>
        /// Media tags.
        /// </summary>
        public ICollection<MediaTag> MediaTags { get; set; }

        /// <summary>
        /// Photograph for info block.
        /// </summary>
        public Media MainPhoto { get; set; }
    }
}
