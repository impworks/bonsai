using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing relations.
    /// </summary>
    [Route("admin/relations")]
    public class RelationsController: AdminControllerBase
    {
        public RelationsController(RelationsManagerService rels, AppDbContext db)
        {
            _rels = rels;
            _db = db;
        }

        private readonly RelationsManagerService _rels;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of relations.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index(RelationsListRequestVM request)
        {
            var rels = await _rels.GetRelationsAsync(request).ConfigureAwait(false);
            return View(rels);
        }
    }
}
