using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for ajax lookups.
    /// </summary>
    [Route("admin/suggest")]
    public class SuggestController: AdminControllerBase
    {
        public SuggestController(SuggestService suggest)
        {
            _suggest = suggest;
        }

        private readonly SuggestService _suggest;

        /// <summary>
        /// Suggests relation types.
        /// </summary>
        [HttpGet]
        [Route("relations")]
        public async Task<ActionResult> SuggestRelations(PageType pageType)
        {
            var names = EnumHelper.GetEnumDescriptions<RelationType>();
            var relTypes = RelationHelper.SuggestRelationTypes(pageType);
            var data = relTypes.Select(x => new ListItem<RelationType>(x, names[x]));
            return Json(data);
        }

        /// <summary>
        /// Suggests pages for relations.
        /// </summary>
        [HttpGet]
        [Route("pages")]
        public async Task<ActionResult> SuggestPages(string query, PageType pageType, RelationType relType)
        {
            var pages = await _suggest.SuggestRelatedPagesAsync(query, pageType, relType).ConfigureAwait(false);

            foreach (var page in pages)
                page.MainPhotoPath = Url.Content(PageHelper.GetPageImageUrl(page.Type, page.MainPhotoPath));

            return Json(pages);
        }
    }
}
