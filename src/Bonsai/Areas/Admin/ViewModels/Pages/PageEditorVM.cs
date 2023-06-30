using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Mapster;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Detailed information about a page's contents.
    /// </summary>
    public class PageEditorVM: IMapped, IVersionable
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Page title.
        /// </summary>
        [Required(ErrorMessage = "Необходимо ввести название страницы")]
        [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; set; }

        /// <summary>
        /// Type of the entity described by this page.
        /// </summary>
        public PageType Type { get; set; }

        /// <summary>
        /// Free text description of the entity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Serialized collection of facts related to current entity.
        /// </summary>
        public string Facts { get; set; }

        /// <summary>
        /// Key of the main photo.
        /// </summary>
        public string MainPhotoKey { get; set; }

        /// <summary>
        /// Aliases for current page.
        /// </summary>
        public string Aliases { get; set; }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Page, PageEditorVM>()
                  .Map(x => x.Id, x => x.Id)
                  .Map(x => x.Title, x => x.Title)
                  .Map(x => x.Type, x => x.Type)
                  .Map(x => x.Description, x => x.Description)
                  .Map(x => x.Facts, x => x.Facts)
                  .Map(x => x.MainPhotoKey, x => x.MainPhoto.Key)
                  .Map(x => x.Aliases, x => x.Aliases != null ? JsonConvert.SerializeObject(x.Aliases.OrderBy(y => y.Order).Select(y => y.Title)) : null);

            config.NewConfig<PageEditorVM, Page>()
                  .Map(x => x.Title, x => x.Title)
                  .Map(x => x.Type, x => x.Type)
                  .Map(x => x.Description, x => x.Description)
                  .Map(x => x.Facts, x => x.Facts)
                  .Map(x => x.LastUpdateDate, x => DateTimeOffset.Now)
                  .Map(x => x.Key, x => PageHelper.EncodeTitle(x.Title))
                  .IgnoreNonMapped(true);
        }
    }
}
