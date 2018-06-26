using System.Collections.Generic;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Admin.Logic.Pages
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
        public async Task<PageValidationResult> ValidateAsync(Page page, string rawRelations, string rawFacts)
        {
            var relations = DeserializeRelations(rawRelations);
            var facts = DeserializeFacts(page.Type, rawFacts);

            return new PageValidationResult();
        }

        #region Helpers

        /// <summary>
        /// Deserializes the relation data from the JSON representation.
        /// </summary>
        private IReadOnlyList<PageRelationVM> DeserializeRelations(string rawRelations)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<PageRelationVM>>(rawRelations);
            }
            catch (JsonException)
            {
                throw new ValidationException(nameof(Page.Relations), "Данные об отношениях имеют некорректный формат!");
            }
        }

        /// <summary>
        /// Deserializes the fact data.
        /// </summary>
        private IReadOnlyList<FactModelBase> DeserializeFacts(PageType type, string rawFacts)
        {
            try
            {
                var result = new List<FactModelBase>();
                if (string.IsNullOrEmpty(rawFacts))
                    return result;

                var pageFacts = JObject.Parse(rawFacts);

                foreach(var group in FactDefinitions.Groups[type])
                {
                    foreach(var fact in group.Facts)
                    {
                        var key = group.Id + "." + fact.Id;
                        var factInfo = pageFacts[key];

                        if(factInfo == null)
                            continue;

                        var vm = (FactModelBase)JsonConvert.DeserializeObject(factInfo.ToString(), fact.Kind);
                        vm.Definition = fact;

                        result.Add(vm);
                    }
                }

                return result;
            }
            catch (JsonException)
            {
                throw new ValidationException(nameof(Page.Facts), "Данные о фактах имеют некорректный формат!");
            }
        }

        #endregion
    }
}
