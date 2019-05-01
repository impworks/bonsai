using System;

namespace Bonsai.Areas.Admin.Logic.MediaHandlers
{
    /// <summary>
    /// Additional data about a photo.
    /// </summary>
    public class MediaMetadata
    {
        /// <summary>
        /// Date of the photo or video's origin.
        /// </summary>
        public DateTime? Date { get; set; }
    }
}
