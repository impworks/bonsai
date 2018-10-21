using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Page.InfoBlock;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Renderer for page-related changesets.
    /// </summary>
    public class PageChangesetRenderer: IChangesetRenderer
    {
        public PageChangesetRenderer(IHtmlHelper html, ViewRenderService viewRenderer, IUrlHelper url)
        {
            _html = html;
            _viewRenderer = viewRenderer;
            _url = url;
        }

        private readonly IHtmlHelper _html;
        private readonly IUrlHelper _url;
        private readonly ViewRenderService _viewRenderer;

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
            var facts = await RenderFactsAsync(data.Type, data.Facts);

            Add(nameof(PageEditorVM.Title), "Название", data.Title);
            Add(nameof(PageEditorVM.MainPhotoKey), "Фото", photoUrl == null ? null : ViewHelper.RenderMediaThumbnail(photoUrl));
            Add(nameof(PageEditorVM.Type), "Тип", isEmpty ? null :  data.Type.GetEnumDescription());
            Add(nameof(PageEditorVM.Description), "Текст", data.Description);
            Add(nameof(PageEditorVM.Aliases), "Псевдонимы", data.Aliases == null ? null : ViewHelper.RenderBulletList(_html, aliases));
            Add(nameof(PageEditorVM.Facts), "Факты", facts);

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

        /// <summary>
        /// Renders the facts for display in changeset viewer.
        /// </summary>
        private async Task<string> RenderFactsAsync(PageType pageType, string rawFacts)
        {
            if (string.IsNullOrEmpty(rawFacts))
                return null;

            var facts = JObject.Parse(rawFacts);
            var vms = new List<FactGroupVM>();

            foreach (var group in FactDefinitions.Groups[pageType])
            {
                var groupVm = new FactGroupVM { Definition = group, Facts = new List<FactModelBase>() };
                foreach (var fact in group.Defs)
                {
                    var key = group.Id + "." + fact.Id;
                    var factInfo = facts[key]?.ToString();

                    if (string.IsNullOrEmpty(factInfo))
                        continue;

                    var factVm = (FactModelBase) JsonConvert.DeserializeObject(factInfo, fact.Kind);
                    factVm.Definition = fact;

                    groupVm.Facts.Add(factVm);
                }

                if (groupVm.Facts.Any())
                    vms.Add(groupVm);
            }

            return await _viewRenderer.RenderToStringAsync("~/Areas/Admin/Views/Changesets/Facts/FactsList.cshtml", vms);
        }

        #endregion
    }
}
