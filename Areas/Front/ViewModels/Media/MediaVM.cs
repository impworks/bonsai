using System;
using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.ViewModels.Media
{
    /// <summary>
    /// Details of a single media file.
    /// </summary>
    public class MediaVM
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// URL of the media file to display inline.
        /// </summary>
        public string PreviewPath { get; set; }

        /// <summary>
        /// URL of the full-sized media (for photos).
        /// </summary>
        public string OriginalPath { get; set; }

        /// <summary>
        /// Title of the document (for documents).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the media (photo, video, etc.).
        /// </summary>
        public MediaType Type { get; set; }

        /// <summary>
        /// Flag indicating that the media has been successfully encoded.
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Description of the media (HTML).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tagged entities on the photo.
        /// </summary>
        public IReadOnlyList<MediaTagVM> Tags { get; set; }

        /// <summary>
        /// Date of the media file's creation.
        /// </summary>
        public FuzzyDate? Date { get; set; }

        /// <summary>
        /// Related location.
        /// </summary>
        public PageTitleVM Location { get; set; }

        /// <summary>
        /// Related event.
        /// </summary>
        public PageTitleVM Event { get; set; }
    }
}
