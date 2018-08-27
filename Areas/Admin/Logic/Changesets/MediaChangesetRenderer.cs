using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Renderer for media-related changesets.
    /// </summary>
    public class MediaChangesetRenderer: IChangesetRenderer
    {
        public MediaChangesetRenderer(IHtmlHelper html, AppDbContext db)
        {
            _html = html;
            _db = db;
        }

        private readonly IHtmlHelper _html;
        private readonly AppDbContext _db;

        #region IChangesetRenderer implementation

        /// <summary>
        /// Supported type of changed entity.
        /// </summary>
        public ChangesetEntityType EntityType => ChangesetEntityType.Media;

        /// <summary>
        /// Renders the properties.
        /// </summary>
        public async Task<IReadOnlyList<ChangePropertyValue>> RenderValuesAsync(string json)
        {
            var result = new List<ChangePropertyValue>();
            var data = JsonConvert.DeserializeObject<MediaEditorVM>(StringHelper.Coalesce(json, "{}"));
            var depicted = JsonConvert.DeserializeObject<MediaTagVM[]>(StringHelper.Coalesce(data.DepictedEntities, "[]"));

            var pageIds = depicted.Where(x => x.PageId != null)
                                  .Select(x => x.PageId.Value)
                                  .ToList();

            var locId = data.Location.TryParse<Guid>();
            var eventId = data.Event.TryParse<Guid>();

            if (locId != Guid.Empty) pageIds.Add(locId);
            if (eventId != Guid.Empty) pageIds.Add(eventId);

            var namesLookup = await _db.Pages
                                       .Where(x => pageIds.Contains(x.Id))
                                       .ToDictionaryAsync(x => x.Id, x => x.Title);

            var deps = depicted.Select(x => string.Format("{0} ({1})", namesLookup[x.PageId ?? Guid.Empty] ?? x.ObjectTitle, x.Coordinates));

            Add(nameof(MediaEditorVM.Title), "Название", data.Title);
            Add(nameof(MediaEditorVM.Date), "Дата", data.Date != null ? FuzzyDate.Parse(data.Date).ReadableDate : null);
            Add(nameof(MediaEditorVM.Description), "Описание", data.Description);
            Add(nameof(MediaEditorVM.Location), "Место", namesLookup.TryGetValue(locId) ?? data.Location);
            Add(nameof(MediaEditorVM.Event), "Событие", namesLookup.TryGetValue(eventId) ?? data.Event);
            Add(nameof(MediaEditorVM.DepictedEntities), "Отметки", depicted.Length == 0 ? null : ViewHelper.RenderBulletList(_html, deps));

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
            return null;
        }

        #endregion
    }
}
