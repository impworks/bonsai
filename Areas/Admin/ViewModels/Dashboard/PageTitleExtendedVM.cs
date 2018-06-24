using System;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Dashboard
{
    /// <summary>
    /// Information about the page with a tiny preview image, if any.
    /// </summary>
    public class PageTitleExtendedVM: PageTitleVM, IMapped
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Page's main image.
        /// </summary>
        public string MainPhotoPath { get; set; }

        /// <summary>
        /// Last update.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }

        /// <summary>
        /// Creation date.
        /// </summary>
        public DateTimeOffset CreateDate { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Page, PageTitleExtendedVM>()
                   .ForMember(x => x.MainPhotoPath, opt => opt.MapFrom(x => x.MainPhoto.FilePath));
        }
    }
}
