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
        public PageType Type { get; set; }

        /// <summary>
        /// Free text description of the entity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current entity.
        /// </summary>
        public string Facts { get; set; }

        /// <summary>
        /// Aliases to current page.
        /// </summary>
        public ICollection<PageAlias> Aliases { get; set; }
        
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

        /// <summary>
        /// FK of the main photo.
        /// </summary>
        public Guid? MainPhotoId { get; set; }

        /// <summary>
        /// Date of the page's creation.
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Date of the page's last revision.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }

        /// <summary>
        /// Related layout of the family tree.
        /// </summary>
        public TreeLayout TreeLayout { get; set; }

        /// <summary>
        /// FK of the tree layout.
        /// </summary>
        public Guid? TreeLayoutId { get; set; }

        /// <summary>
        /// Flag indicating that this resource is removed.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
