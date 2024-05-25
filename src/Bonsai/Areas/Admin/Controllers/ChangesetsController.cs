using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Bonsai.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers;

/// <summary>
/// Controller for managing changesets.
/// </summary>
[Route("admin/changes")]
public class ChangesetsController(ChangesetsManagerService changes, AppDbContext db) : AdminControllerBase
{
    protected override Type ListStateType => typeof(ChangesetsListRequestVM);

    #region Public methods

    /// <summary>
    /// Displays the list of all changesets.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> Index(ChangesetsListRequestVM request)
    {
        PersistListState(request);
        var vm = await changes.GetChangesetsAsync(request);
        return View(vm);
    }

    /// <summary>
    /// Displays the information about a particular changeset.
    /// </summary>
    [HttpGet]
    [Route("details")]
    public async Task<ActionResult> Details(Guid id)
    {
        var vm = await changes.GetChangesetDetailsAsync(id);
        return View(vm);
    }

    /// <summary>
    /// Displays the changeset revert confirmation.
    /// </summary>
    [HttpGet]
    [Route("revert")]
    public async Task<ActionResult> Revert(Guid id)
    {
        var vm = await changes.GetChangesetDetailsAsync(id);
        if (!vm.CanRevert)
            throw new OperationException(Texts.Admin_Changesets_CannotRevertMessage);

        return View(vm);
    }

    /// <summary>
    /// Reverts the edit to its original state.
    /// </summary>
    [HttpPost]
    [Route("revert")]
    public async Task<ActionResult> Revert(Guid id, bool confirm)
    {
        var editVm = await changes.GetChangesetDetailsAsync(id);
        if (!editVm.CanRevert)
            throw new OperationException(Texts.Admin_Changesets_CannotRevertMessage);

        await changes.RevertChangeAsync(id, User);
        await db.SaveChangesAsync();

        return RedirectToSuccess(Texts.Admin_Changesets_RevertedMessage);
    }

    #endregion
}