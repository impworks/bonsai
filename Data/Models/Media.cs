using System;

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
        /// Type of the media file.
        /// </summary>
        public virtual MediaType Type { get; set; }

        /// <summary>
        /// Path to the file on the server.
        /// </summary>
        public virtual string FilePath { get; set; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        public virtual string FileName { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current media file.
        /// </summary>
        public virtual string Facts { get; set; }
    }
}
