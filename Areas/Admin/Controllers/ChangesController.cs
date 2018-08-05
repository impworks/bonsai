using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.History;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing changesets.
    /// </summary>
    [Route("admin/changes")]
    public class ChangesController: AdminControllerBase
    {
        public ChangesController(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of all changesets.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(ChangesetListRequestVM vm)
        {
            return View();
        }
    }
}
