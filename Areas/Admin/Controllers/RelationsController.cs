using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing relations.
    /// </summary>
    [Route("admin/relations")]
    public class RelationsController: AdminControllerBase
    {
        public RelationsController(RelationsManagerService rels, PagesManagerService pages, AppDbContext db)
        {
            _rels = rels;
            _pages = pages;
            _db = db;
        }

        private readonly RelationsManagerService _rels;
        private readonly PagesManagerService _pages;
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
            return await ViewEditorFormAsync(new RelationEditorVM { Type = RelationType.Child }).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to create a new page.
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create(RelationEditorVM vm)
        {
            if(!ModelState.IsValid)
                return await ViewEditorFormAsync(vm).ConfigureAwait(false);

            try
            {
                await _rels.CreateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Связь создана");
            }
            catch(ValidationException ex)
            {
                SetModelState(ex);
                return await ViewEditorFormAsync(vm).ConfigureAwait(false);
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
            return await ViewEditorFormAsync(vm).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to update the existing relation.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(RelationEditorVM vm)
        {
            if(!ModelState.IsValid)
                return await ViewEditorFormAsync(vm);

            try
            {
                await _rels.UpdateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                return RedirectToSuccess("Связь обновлена");
            }
            catch(ValidationException ex)
            {
                SetModelState(ex);
                return await ViewEditorFormAsync(vm).ConfigureAwait(false);
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

        /// <summary>
        /// Returns editor properties for the selected relation type.
        /// </summary>
        [HttpGet]
        [Route("editorProps")]
        public async Task<ActionResult> EditorProperties(RelationType relType)
        {
            return Json(_rels.GetPropertiesForRelationType(relType));
        }

        #region Helpers

        /// <summary>
        /// Displays the editor.
        /// </summary>
        private async Task<ActionResult> ViewEditorFormAsync(RelationEditorVM vm)
        {
            var pageLookup = await _pages.FindPagesByIdsAsync(new[] {vm.SourceId, vm.DestinationId, vm.EventId})
                                         .ConfigureAwait(false);

            ViewBag.Data = new RelationEditorDataVM
            {
                IsNew = vm.Id == Guid.Empty,
                SourceItem = GetPageLookup(vm.SourceId),
                DestinationItem = GetPageLookup(vm.DestinationId),
                EventItem = GetPageLookup(vm.EventId),

                Properties = _rels.GetPropertiesForRelationType(vm.Type),
                RelationTypes = EnumHelper.GetEnumValues<RelationType>()
                                          .Select(x => new SelectListItem
                                          {
                                              Value = x.ToString(),
                                              Text = x.GetEnumDescription(),
                                              Selected = x == vm.Type
                                          })
            };

            return View("Editor", vm);

            SelectListItem[] GetPageLookup(Guid? pageId)
            {
                return pageLookup.TryGetValue(pageId ?? Guid.Empty, out var page)
                    ? new[] {new SelectListItem {Selected = true, Text = page.Title, Value = page.Id.ToString()}}
                    : Array.Empty<SelectListItem>();
            }
        }

        #endregion
    }
}
