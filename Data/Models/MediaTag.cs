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
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Tagged media.
        /// </summary>
        public virtual Media Media { get; set; }

        /// <summary>
        /// Tagged entity link.
        /// </summary>
        public virtual Page Object { get; set; }

        /// <summary>
        /// Type of media tag.
        /// </summary>
        public virtual MediaTagType Type { get; set; }

        /// <summary>
        /// Name or title of the tagged entity.
        /// </summary>
        [StringLength(500)]
        public virtual string ObjectTitle { get; set; }

        /// <summary>
        /// Semicolon-separated coordinates of the tag.
        /// </summary>
        [StringLength(100)]
        public virtual string Coordinates { get; set; }
    }
}
