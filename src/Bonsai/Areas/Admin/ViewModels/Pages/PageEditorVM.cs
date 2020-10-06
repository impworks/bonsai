using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
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

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Page, PageEditorVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.Description, x => x.Description)
                   .MapMember(x => x.Facts, x => x.Facts)
                   .MapMember(x => x.MainPhotoKey, x => x.MainPhoto.Key)
                   .MapMember(x => x.Aliases, x => JsonConvert.SerializeObject(x.Aliases.OrderBy(y => y.Order).Select(y => y.Title)));

            profile.CreateMap<PageEditorVM, Page>()
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.Description, x => x.Description)
                   .MapMember(x => x.Facts, x => x.Facts)
                   .MapMember(x => x.LastUpdateDate, x => DateTimeOffset.Now)
                   .MapMember(x => x.Key, x => PageHelper.EncodeTitle(x.Title))
                   .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
