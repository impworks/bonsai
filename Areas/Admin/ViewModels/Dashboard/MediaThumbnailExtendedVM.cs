using System;
using AutoMapper;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Media;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Additional information about a media file.
    /// </summary>
    public class MediaThumbnailExtendedVM: MediaThumbnailVM, IMapped
    {
        /// <summary>
        /// Date of the media file's upload.
        /// </summary>
        public DateTimeOffset UploadDate { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Media, MediaThumbnailExtendedVM>()
                   .ForMember(x => x.OriginDate, opt => opt.MapFrom(x => FuzzyDate.TryParse(x.Date)))
                   .ForMember(y => y.ThumbnailUrl, opt => opt.MapFrom(x => MediaPresenterService.GetSizedMediaPath(x.FilePath, MediaSize.Small)));
        }
    }
}
