using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Detailed information about a page's contents.
    /// </summary>
    public class PageEditorVM: IMapped
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
        /// Serialized collection of relations to current entity.
        /// </summary>
        public string Relations { get; set; }

        /// <summary>
        /// Key of the main photo.
        /// </summary>
        public string MainPhotoKey { get; set; }

        /// <summary>
        /// Aliases for current page.
        /// </summary>
        public ICollection<string> Aliases { get; set; }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<Page, PageEditorVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.Description, x => x.Description)
                   .MapMember(x => x.Facts, x => x.Facts)
                   .MapMember(x => x.Aliases, x => x.Aliases.Select(y => y.Key).ToList())
                   .MapMember(x => x.MainPhotoKey, x => x.MainPhoto.Key)
                   .MapMember(
                       x => x.Relations,
                       x => JsonConvert.SerializeObject(
                           x.Relations
                            .Where(y => !y.IsComplementary)
                            .AsQueryable()
                            .ProjectTo<PageRelationVM>()
                       )
                   );

            profile.CreateMap<PageEditorVM, Page>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.Description, x => x.Description)
                   .MapMember(x => x.Facts, x => x.Facts)
                   .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
