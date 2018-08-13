using System;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Code.Services.Elastic;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for handling pages.
    /// </summary>
    [Route("admin/pages")]
    public class PagesController: AdminControllerBase
    {
        public PagesController(PagesManagerService pages, ElasticService elastic, AppDbContext db)
        {
            _pages = pages;
            _elastic = elastic;
            _db = db;
        }

        private readonly PagesManagerService _pages;
        private readonly ElasticService _elastic;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of pages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PagesListRequestVM request)
        {
            var vm = await _pages.GetPagesAsync(request).ConfigureAwait(false);
            return View(vm);
        }

        /// <summary>
        /// Dispays the editor form for a new page.
        /// </summary>
        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> Create([FromQuery]PageType type = PageType.Person)
        {
            return ViewEditorForm(new PageEditorVM {Type = type});
        }

        /// <summary>
        /// Attempts to create a new page.
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create(PageEditorVM vm)
        {
            if(!ModelState.IsValid)
                return ViewEditorForm(vm);

            try
            {
                var page = await _pages.CreateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);
                await _elastic.AddPageAsync(page).ConfigureAwait(false);

                return RedirectToSuccess("Страница создана");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return ViewEditorForm(vm);
            }
        }

        /// <summary>
        /// Displays the editor for an existing page.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(Guid id)
        {
            var vm = await _pages.RequestUpdateAsync(id).ConfigureAwait(false);
            return ViewEditorForm(vm);
        }

        /// <summary>
        /// Attempts to update the existing page.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(PageEditorVM vm)
        {
            if(!ModelState.IsValid)
                return ViewEditorForm(vm);

            try
            {
                var page = await _pages.UpdateAsync(vm, User).ConfigureAwait(false);
                await _db.SaveChangesAsync().ConfigureAwait(false);

                await _elastic.AddPageAsync(page).ConfigureAwait(false);

                return RedirectToSuccess("Страница обновлена");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return ViewEditorForm(vm);
            }
        }


        /// <summary>
        /// Removes the page file.
        /// </summary>
        [HttpGet]
        [Route("remove")]
        public async Task<ActionResult> Remove(Guid id)
        {
            var vm = await _pages.RequestRemoveAsync(id);
            return View(vm);
        }

        /// <summary>
        /// Removes the page file.
        /// </summary>
        [HttpPost]
        [Route("remove")]
        public async Task<ActionResult> Remove(Guid id, bool confirm)
        {
            var page = await _pages.RemoveAsync(id, User);
            await _db.SaveChangesAsync();

            await _elastic.RemovePageAsync(page);

            return RedirectToSuccess("Медиа-файл удален");
        }

        #region Helpers

        /// <summary>
        /// Displays the editor form.
        /// </summary>
        private ActionResult ViewEditorForm(PageEditorVM vm)
        {
            var groups = new[] // FactDefinitions.Groups[vm.Type]
            {
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    true,
                    new FactDefinition<DateFactModel>("Date", "Дата рождения", "Дата"),
                    new FactDefinition<StringFactModel>("Place", "Место рождения", "Место")
                )
            };
            var editorTpls = groups.SelectMany(x => x.Facts)
                                   .Select(x => x.Kind)
                                   .Distinct()
                                   .Select(x => (Activator.CreateInstance(x) as FactModelBase).EditTemplatePath)
                                   .ToList();

            ViewBag.Data = new PageEditorDataVM
            {
                IsNew = vm.Id == Guid.Empty,
                PageTypes = ViewHelper.GetEnumSelectList(vm.Type),
                FactGroups = groups,
                EditorTemplates = editorTpls
            };

            return View("Editor", vm);
        }

        #endregion
    }
}
