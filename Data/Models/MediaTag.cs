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
        public Media Media { get; set; }

        /// <summary>
        /// Tagged entity link.
        /// </summary>
        public Page Object { get; set; }

        /// <summary>
        /// Type of media tag.
        /// </summary>
        public MediaTagType Type { get; set; }

        /// <summary>
        /// Name or title of the tagged entity.
        /// </summary>
        [StringLength(500)]
        public string ObjectTitle { get; set; }

        /// <summary>
        /// Semicolon-separated coordinates of the tag.
        /// </summary>
        [StringLength(100)]
        public string Coordinates { get; set; }
    }
}
