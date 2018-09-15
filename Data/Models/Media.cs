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
        [Required]
        [StringLength(30)]
        public string Key { get; set; }

        /// <summary>
        /// Type of the media file.
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// MIME type of the file.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string MimeType { get; set; }

        /// <summary>
        /// Path to the file on the server.
        /// </summary>
        [Required]
        [StringLength(300)]
        public string FilePath { get; set; }

        /// <summary>
        /// Media creation date.
        /// </summary>
        [StringLength(30)]
        public string Date { get; set; }

        /// <summary>
        /// Title of the media (for documents).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Markdown description of the media file.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tagged entities on the media.
        /// </summary>
        public ICollection<MediaTag> Tags { get; set; }

        /// <summary>
        /// Date of the media's upload to the server.
        /// </summary>
        public DateTimeOffset UploadDate { get; set; }

        /// <summary>
        /// Reference to the user who has uploaded this media.
        /// </summary>
        public AppUser Uploader { get; set; }

        /// <summary>
        /// Flag indicating that this resource has been encoded and can be viewed.
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Flag indicating that this resource is removed.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
