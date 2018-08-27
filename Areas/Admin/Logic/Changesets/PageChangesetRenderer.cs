using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Renderer for page-related changesets.
    /// </summary>
    public class PageChangesetRenderer: IChangesetRenderer
    {
        public PageChangesetRenderer(IHtmlHelper html, IUrlHelper url)
        {
            _html = html;
            _url = url;
        }

        private readonly IHtmlHelper _html;
        private readonly IUrlHelper _url;

        #region IChangesetRenderer implementation

        /// <summary>
        /// Supported type of changed entity.
        /// </summary>
        public ChangesetEntityType EntityType => ChangesetEntityType.Page;

        /// <summary>
        /// Renders the properties.
        /// </summary>
        public async Task<IReadOnlyList<ChangePropertyValue>> RenderValuesAsync(string json)
        {
            var result = new List<ChangePropertyValue>();
            var isEmpty = string.IsNullOrEmpty(json);
            var data = JsonConvert.DeserializeObject<PageEditorVM>(StringHelper.Coalesce(json, "{}"));
            var aliases = JsonConvert.DeserializeObject<string[]>(data.Aliases ?? "[]");
            var photoUrl = GetMediaThumbnailPath(data.MainPhotoKey);

            Add(nameof(PageEditorVM.Title), "Название", data.Title);
            Add(nameof(PageEditorVM.MainPhotoKey), "Фото", photoUrl == null ? null : ViewHelper.RenderMediaThumbnail(photoUrl));
            Add(nameof(PageEditorVM.Type), "Тип", isEmpty ? null :  data.Type.GetEnumDescription());
            Add(nameof(PageEditorVM.Description), "Текст", data.Description);
            Add(nameof(PageEditorVM.Aliases), "Псевдонимы", data.Aliases == null ? null : ViewHelper.RenderBulletList(_html, aliases));

            // todo: facts!

            return result;

            void Add(string prop, string name, string value)
            {
                result.Add(new ChangePropertyValue(prop, name, value));
            }
        }

        /// <summary>
        /// Returns custom diffs.
        /// </summary>
        public string GetCustomDiff(string propName, string oldValue, string newValue)
        {
            if (propName == nameof(PageEditorVM.MainPhotoKey))
            {
                var sb = new StringBuilder();

                if (oldValue != null)
                    sb.Append($@"<del class=""img"">{oldValue}</del>");

                if (newValue != null)
                    sb.Append($@"<ins class=""img"">{newValue}</ins>");

                return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// Returns the full path for a photo.
        /// </summary>
        private string GetMediaThumbnailPath(string key)
        {
            if (key == null)
                return null;

            // hack: using original path with fake extension to avoid a DB query because thumbnails are always JPG
            var path = MediaPresenterService.GetSizedMediaPath($"~/media/{key}.fake", MediaSize.Small);
            return _url.Content(path);
        }

        #endregion
    }
}
