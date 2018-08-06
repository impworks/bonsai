using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing changesets.
    /// </summary>
    [Route("admin/changes")]
    public class ChangesetsController: AdminControllerBase
    {
        public ChangesetsController(ChangesetsManagerService changes)
        {
            _changes = changes;
        }

        private ChangesetsManagerService _changes;

        /// <summary>
        /// Displays the list of all changesets.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(ChangesetsListRequestVM request)
        {
            var vm = await _changes.GetChangesetsAsync(request);
            return View(vm);
        }

        [HttpGet]
        [Route("details")]
        public async Task<ActionResult> Details(Guid id)
        {
            var vm = await _changes.GetChangesetDetailsAsync(id);
            return View(vm);
        }
    }
}
