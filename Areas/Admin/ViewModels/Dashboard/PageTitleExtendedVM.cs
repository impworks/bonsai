using System;
using AutoMapper;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
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
        /// Date of the page's last update.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }

        /// <summary>
        /// Date of the page's creation.
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Page, PageTitleExtendedVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Key, x => x.Key)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.MainPhotoPath, x => x.MainPhoto.FilePath)
                   .MapMember(x => x.LastUpdateDate, x => x.LastUpdateDate)
                   .MapMember(x => x.CreationDate, x => x.CreationDate);
        }
    }
}
