using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.Utils;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for ajax lookups.
    /// </summary>
    [Route("admin")]
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
        [Route("suggest/relations")]
        public ActionResult SuggestRelations()
        {
            var names = EnumHelper.GetEnumDescriptions<RelationType>();
            var data = names.Select(x => new ListItem<RelationType>(x.Key, x.Value));
            return Json(data);
        }

        /// <summary>
        /// Suggests pages for relation destination / media tag.
        /// </summary>
        [HttpGet]
        [Route("suggest/pages")]
        public async Task<ActionResult> SuggestPages([FromQuery] PickRequestVM<PageType> vm)
        {
            var pages = await _suggest.SuggestPagesAsync(vm);
            return Json(pages);
        }

        /// <summary>
        /// Suggests pages for relation source.
        /// </summary>
        [HttpGet]
        [Route("suggest/pages/rel")]
        public async Task<ActionResult> SuggestPagesForRelationSource([FromQuery] RelationSuggestQueryVM vm)
        {
            var pages = await _suggest.SuggestRelationPagesAsync(vm);
            return Json(pages);
        }

        /// <summary>
        /// Returns data for page picker.
        /// </summary>
        [HttpGet]
        [Route("pick/pages")]
        public async Task<ActionResult> PickPages([FromQuery] PickRequestVM<PageType> vm)
        {
            var media = await _suggest.GetPickablePagesAsync(vm);
            return Json(media);
        }

        /// <summary>
        /// Returns data for media picker.
        /// </summary>
        [HttpGet]
        [Route("pick/media")]
        public async Task<ActionResult> PickMedia([FromQuery] PickRequestVM<MediaType> vm)
        {
            var media = await _suggest.GetPickableMediaAsync(vm);
            return Json(media);
        }
    }
}
