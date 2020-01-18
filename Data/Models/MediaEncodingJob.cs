using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Information about a media file to be processed.
    /// </summary>
    public class MediaEncodingJob
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// FK of the media file.
        /// </summary>
        public Guid MediaId { get; set; }

        /// <summary>
        /// Encoded media.
        /// </summary>
        public Media Media { get; set; }
    }
}
