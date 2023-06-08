using System;
using Bonsai.Code.Infrastructure;
using Mapster;

namespace Bonsai.Areas.Front.ViewModels.Page
{
    /// <summary>
    /// Information about the page with a tiny preview image, if any.
    /// </summary>
    public class PageTitleExtendedVM: PageTitleVM, IMapped
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        public new Guid Id { get; set; }

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

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Data.Models.Page, PageTitleExtendedVM>()
                   .Map(x => x.Id, x => x.Id)
                   .Map(x => x.Title, x => x.Title)
                   .Map(x => x.Key, x => x.Key)
                   .Map(x => x.Type, x => x.Type)
                   .Map(x => x.MainPhotoPath, x => x.MainPhoto.FilePath)
                   .Map(x => x.LastUpdateDate, x => x.LastUpdateDate)
                   .Map(x => x.CreationDate, x => x.CreationDate);
        }
    }
}
