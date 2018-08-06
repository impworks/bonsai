using System.Collections.Generic;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Renderer for page-related changesets.
    /// </summary>
    public class PageChangesetRenderer: IChangesetRenderer
    {
        public PageChangesetRenderer(IHtmlHelper html)
        {
            _html = html;
        }

        private readonly IHtmlHelper _html;

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

            Add("Название", data.Title);
            Add("Тип", isEmpty ? null :  data.Type.GetEnumDescription());
            Add("Текст", data.Description);
            Add("Псевдонимы", data.Aliases == null ? null : ViewHelper.RenderBulletList(_html, data.Aliases));

            // todo: facts!

            return result;

            void Add(string name, string value)
            {
                result.Add(new ChangePropertyValue(name, value));
            }
        }

        #endregion
    }
}
