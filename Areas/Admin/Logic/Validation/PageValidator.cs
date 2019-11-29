using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Admin.Logic.Validation
{
    /// <summary>
    /// The validator that checks relation/fact consistency.
    /// </summary>
    public class PageValidator
    {
        public PageValidator(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Checks the current relations and prepares them for database serialization.
        /// </summary>
        public async Task ValidateAsync(Page page, string rawFacts)
        {
            var context = await RelationContext.LoadContextAsync(_db);
            AugmentRelationContext(context, page, rawFacts);

            var core = new ValidatorCore();
            core.Validate(context, new [] { page.Id });
            core.ThrowIfInvalid(context, nameof(PageEditorVM.Facts));
        }

        #region Helpers

        /// <summary>
        /// Adds information from the current page to the context.
        /// </summary>
        private void AugmentRelationContext(RelationContext context, Page page, string rawFacts)
        {
            var facts = ParseFacts(page.Type, rawFacts);
            var excerpt = new RelationContext.PageExcerpt
            {
                Id = page.Id,
                Key = page.Key,
                Type = page.Type,
                Title = page.Title,
                Gender = Parse<bool>("Bio.Gender", "IsMale"),
                BirthDate = Parse<FuzzyDate>("Birth.Date", "Value"),
                DeathDate = Parse<FuzzyDate>("Death.Date", "Value"),
            };

            context.Augment(excerpt);

            T? Parse<T>(params string[] parts) where T : struct
            {
                var curr = (JToken) facts;

                foreach(var part in parts)
                    curr = curr?[part];

                var value = curr?.ToString();

                if(typeof(T) == typeof(FuzzyDate))
                    return (T?)(object)FuzzyDate.TryParse(value);

                return value.TryParse<T?>();
            }
        }

        /// <summary>
        /// Deserializes the fact data.
        /// </summary>
        private JObject ParseFacts(PageType type, string rawFacts)
        {
            if (string.IsNullOrEmpty(rawFacts))
                return new JObject();

            var pageFacts = ParseRaw(rawFacts);
            foreach (var prop in pageFacts)
            {
                var def = FactDefinitions.TryGetDefinition(type, prop.Key);
                if (def == null)
                    throw new ValidationException(nameof(Page.Facts), $"Тип факта {prop.Key} не существует!");

                try
                {
                    var model = JsonConvert.DeserializeObject(prop.Value.ToString(), def.Kind) as FactModelBase;

                    if (!model.IsValid)
                        throw new Exception();
                }
                catch (Exception ex) when (!(ex is ValidationException))
                {
                    throw new ValidationException(nameof(Page.Facts), $"Некорректно заполнен факт {prop.Key}!");
                }
            }

            return pageFacts;

            JObject ParseRaw(string raw)
            {
                try
                {
                    var json = JObject.Parse(raw);
                    return JsonHelper.RemoveEmptyChildren(json);
                }
                catch
                {
                    throw new ValidationException(nameof(Page.Facts), "Факты имеют некорректный формат!");
                }
            }
        }

        #endregion
    }
}
