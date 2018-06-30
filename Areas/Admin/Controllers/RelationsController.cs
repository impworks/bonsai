using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.Utils.Validation;
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

        /// <summary>
        /// Dispays the editor form for a new relation.
        /// </summary>
        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> Create()
        {
            return ViewEditorForm(new RelationEditorVM());
        }

        /// <summary>
        /// Attempts to create a new page.
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create(RelationEditorVM vm)
        {
            if(!ModelState.IsValid)
                return ViewEditorForm(vm);

            try
            {
                await _rels.CreateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Связь создана");
            }
            catch(ValidationException ex)
            {
                SetModelState(ex);
                return ViewEditorForm(vm);
            }
        }

        /// <summary>
        /// Displays the editor for an existing relation.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(Guid id)
        {
            var vm = await _rels.RequestUpdateAsync(id).ConfigureAwait(false);
            return ViewEditorForm(vm);
        }

        /// <summary>
        /// Attempts to update the existing relation.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(RelationEditorVM vm)
        {
            if(!ModelState.IsValid)
                return ViewEditorForm(vm);

            try
            {
                await _rels.UpdateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Связь обновлена");
            }
            catch(ValidationException ex)
            {
                SetModelState(ex);
                return ViewEditorForm(vm);
            }
        }

        /// <summary>
        /// Removes the relation.
        /// </summary>
        [HttpPost]
        [Route("remove")]
        public async Task<ActionResult> Remove(Guid id, bool confirm)
        {
            await _rels.RemoveAsync(id, User).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);

            return RedirectToSuccess("Связь удалена");
        }

        #region Helpers

        /// <summary>
        /// Displays the editor.
        /// </summary>
        public ActionResult ViewEditorForm(RelationEditorVM vm)
        {
            return View("Editor", vm);
        }

        #endregion
    }
}
