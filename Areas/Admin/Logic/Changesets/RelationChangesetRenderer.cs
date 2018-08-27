using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    /// <summary>
    /// Renderer for relation changesets.
    /// </summary>
    public class RelationChangesetRenderer: IChangesetRenderer
    {
        public RelationChangesetRenderer(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        #region IChangesetRenderer implementation

        /// <summary>
        /// Supported relation type.
        /// </summary>
        public ChangesetEntityType EntityType => ChangesetEntityType.Relation;

        /// <summary>
        /// Renders the property values.
        /// </summary>
        public async Task<IReadOnlyList<ChangePropertyValue>> RenderValuesAsync(string json)
        {
            var result = new List<ChangePropertyValue>();
            var data = JsonConvert.DeserializeObject<RelationEditorVM>(StringHelper.Coalesce(json, "{}"));

            var pageIds = new[] {data.SourceId, data.DestinationId, data.EventId}.Where(x => x != null)
                                                                                 .Select(x => x.Value)
                                                                                 .ToList();

            var namesLookup = await _db.Pages
                                       .Where(x => pageIds.Contains(x.Id))
                                       .ToDictionaryAsync(x => x.Id, x => x.Title);

            Add(nameof(RelationEditorVM.DestinationId), "Основная страница", namesLookup.TryGetValue(data.DestinationId ?? Guid.Empty));
            Add(nameof(RelationEditorVM.SourceId), "Связанная страница", namesLookup.TryGetValue(data.SourceId ?? Guid.Empty));
            Add(nameof(RelationEditorVM.Type), "Тип связи", string.IsNullOrEmpty(json) ? null : data.Type.GetEnumDescription());
            Add(nameof(RelationEditorVM.EventId), "Событие", namesLookup.TryGetValue(data.EventId ?? Guid.Empty));
            Add(nameof(RelationEditorVM.DurationStart), "Начало", FuzzyDate.TryParse(data.DurationStart)?.ReadableDate);
            Add(nameof(RelationEditorVM.DurationEnd), "Конец", FuzzyDate.TryParse(data.DurationEnd)?.ReadableDate);
            
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
