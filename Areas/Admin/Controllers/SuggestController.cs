using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
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
        /// Suggests pages for relation source / media tag.
        /// </summary>
        [HttpGet]
        [Route("suggest/pages")]
        public async Task<ActionResult> SuggestPages(string query, int? count = null, int? offset = null, PageType[] types = null)
        {
            var pages = await _suggest.SuggestPagesAsync(query, types);
            return Json(pages);
        }

        /// <summary>
        /// Returns data for page picker.
        /// </summary>
        [HttpGet]
        [Route("pick/pages")]
        public async Task<ActionResult> PickPages(string query = null, int? count = null, int? offset = null, PageType[] types = null)
        {
            var media = await _suggest.GetPickablePagesAsync(query, count, offset, types);
            return Json(media);
        }

        /// <summary>
        /// Returns data for media picker.
        /// </summary>
        [HttpGet]
        [Route("pick/media")]
        public async Task<ActionResult> PickMedia(string query = null, int? count = null, int? offset = null, MediaType[] types = null)
        {
            var media = await _suggest.GetPickableMediaAsync(query, count, offset, types);
            return Json(media);
        }
    }
}
