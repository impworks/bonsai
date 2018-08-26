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
        [Route("pages")]
        public async Task<ActionResult> SuggestPages(string query, PageType[] types = null)
        {
            var pages = await _suggest.SuggestPagesAsync(query, types);
            return Json(pages);
        }

        [HttpGet]
        [Route("media")]
        public async Task<ActionResult> SuggestMedia(string query = null, int? count = null, int? offset = null, MediaType[] types = null)
        {
            var media = await _suggest.SuggestMediaAsync(query, count, offset, types);
            return Json(media);
        }
    }
}
