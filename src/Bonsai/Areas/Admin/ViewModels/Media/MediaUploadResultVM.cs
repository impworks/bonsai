using System;
using AutoMapper;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;

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

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Data.Models.Media, MediaUploadResultVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Key, x => x.Key)
                   .MapMember(x => x.IsProcessed, x => x.IsProcessed)
                   .MapMember(x => x.ThumbnailPath, x => MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small));
        }
    }
}
