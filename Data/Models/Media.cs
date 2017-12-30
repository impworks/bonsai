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
        public Guid Id { get; set; }

        /// <summary>
        /// Url-friendly key generated from surrogate ID.
        /// </summary>
        [Key]
        [StringLength(30)]
        public string Key { get; set; }

        /// <summary>
        /// Type of the media file.
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Path to the file on the server.
        /// </summary>
        [StringLength(300)]
        public string FilePath { get; set; }

        /// <summary>
        /// Media creation date.
        /// </summary>
        [StringLength(100)]
        public string Date { get; set; }

        /// <summary>
        /// Markdown description of the media file.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current media.
        /// </summary>
        public string Facts { get; set; }

        /// <summary>
        /// Tagged entities on the media.
        /// </summary>
        public ICollection<MediaTag> Tags { get; set; }
    }
}
