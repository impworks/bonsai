using System;
using System.Linq;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Additional information about a media file.
    /// </summary>
    public class MediaThumbnailExtendedVM: MediaThumbnailVM, IMapped
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Date of the media file's upload.
        /// </summary>
        public DateTimeOffset UploadDate { get; set; }

        /// <summary>
        /// Number of tagged entities.
        /// </summary>
        public int MediaTagsCount { get; set; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; set; }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Data.Models.Media, MediaThumbnailExtendedVM>()
                   .Map(x => x.Id, x => x.Id)
                   .Map(x => x.Key, x => x.Key)
                   .Map(x => x.Type, x => x.Type)
                   .Map(x => x.Date, x => FuzzyDate.TryParse(x.Date))
                   .Map(x => x.UploadDate, x => x.UploadDate)
                   .Map(x => x.Title, x => x.Title)
                   .Map(x => x.IsProcessed, x => x.IsProcessed)
                   .Map(x => x.MediaTagsCount, x => x.Tags.Count(y => y.Type == MediaTagType.DepictedEntity))
                   .Map(x => x.ThumbnailUrl, x => MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small));
        }
    }
}
