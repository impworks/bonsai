using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Uploaded photo or video.
    /// </summary>
    public class Media
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Url-friendly key generated from surrogate ID.
        /// </summary>
        [Key]
        [StringLength(50)]
        public virtual string Key { get; set; }

        /// <summary>
        /// Type of the media file.
        /// </summary>
        public virtual MediaType Type { get; set; }

        /// <summary>
        /// Path to the file on the server.
        /// </summary>
        [StringLength(500)]
        public virtual string FilePath { get; set; }

        /// <summary>
        /// Media creation date.
        /// </summary>
        [StringLength(100)]
        public virtual string Date { get; set; }

        /// <summary>
        /// Markdown description of the media file.
        /// </summary>
        [MaxLength]
        public virtual string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current media.
        /// </summary>
        [MaxLength]
        public virtual string Facts { get; set; }

        /// <summary>
        /// Tagged entities on the media.
        /// </summary>
        public virtual ICollection<MediaTag> Tags { get; set; }
    }
}
