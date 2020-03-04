using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing changesets.
    /// </summary>
    [Route("admin/changes")]
    public class ChangesetsController: AdminControllerBase
    {
        public ChangesetsController(ChangesetsManagerService changes, AppDbContext db)
        {
            _changes = changes;
            _db = db;
        }

        private readonly ChangesetsManagerService _changes;
        private readonly AppDbContext _db;

        #region Public methods

        /// <summary>
        /// Displays the list of all changesets.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(ChangesetsListRequestVM request)
        {
            var vm = await _changes.GetChangesetsAsync(request);
            return View(vm);
        }

        /// <summary>
        /// Displays the information about a particular changeset.
        /// </summary>
        [HttpGet]
        [Route("details")]
        public async Task<ActionResult> Details(Guid id)
        {
            var vm = await _changes.GetChangesetDetailsAsync(id);
            return View(vm);
        }

        /// <summary>
        /// Displays the changeset revert confirmation.
        /// </summary>
        [HttpGet]
        [Route("revert")]
        public async Task<ActionResult> Revert(Guid id)
        {
            var vm = await _changes.GetChangesetDetailsAsync(id);
            if (!vm.CanRevert)
                throw new OperationException("Эта правка не может быть отменена");

            return View(vm);
        }

        /// <summary>
        /// Reverts the edit to its original state.
        /// </summary>
        [HttpPost]
        [Route("revert")]
        public async Task<ActionResult> Revert(Guid id, bool confirm)
        {
            var editVm = await _changes.GetChangesetDetailsAsync(id);
            if (!editVm.CanRevert)
                throw new OperationException("Эта правка не может быть отменена");

            await _changes.RevertChangeAsync(id, User);
            await _db.SaveChangesAsync();

            return RedirectToSuccess("Правка была отменена");
        }

        #endregion
    }
}
