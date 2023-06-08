using System;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Infrastructure;
using Mapster;

namespace Bonsai.Areas.Admin.ViewModels.Media
{
    /// <summary>
    /// Results of the media file's upload.
    /// </summary>
    public class MediaUploadResultVM: IMapped
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Full path to preview.
        /// </summary>
        public string ThumbnailPath { get; set; }

        /// <summary>
        /// Flag indicating that the media has been processed completely.
        /// </summary>
        public bool IsProcessed { get; set; }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Data.Models.Media, MediaUploadResultVM>()
                   .Map(x => x.Id, x => x.Id)
                   .Map(x => x.Key, x => x.Key)
                   .Map(x => x.IsProcessed, x => x.IsProcessed)
                   .Map(x => x.ThumbnailPath, x => MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small));
        }
    }
}
