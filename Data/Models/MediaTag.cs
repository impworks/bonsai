using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// A person or entity tagged in a photo or video.
    /// </summary>
    public class MediaTag
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tagged media.
        /// </summary>
        [Required]
        public Media Media { get; set; }

        /// <summary>
        /// Tagged entity (if exists).
        /// </summary>
        public Page Object { get; set; }

        /// <summary>
        /// FK of the tagged entity (if exists).
        /// </summary>
        public Guid? ObjectId { get; set; }

        /// <summary>
        /// Explicit title for tags that don't reference an object.
        /// </summary>
        public string ObjectTitle { get; set; }

        /// <summary>
        /// Type of media tag.
        /// </summary>
        public MediaTagType Type { get; set; }

        /// <summary>
        /// Semicolon-separated coordinates of the tag.
        /// </summary>
        [StringLength(100)]
        public string Coordinates { get; set; }
    }
}
