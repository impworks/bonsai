using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Facts;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Services.Search;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for handling pages.
    /// </summary>
    [Route("admin/pages")]
    public class PagesController: AdminControllerBase
    {
        public PagesController(PagesManagerService pages, ISearchEngine search, AppDbContext db, WorkerAlarmService alarm)
        {
            _pages = pages;
            _search = search;
            _db = db;
            _alarm = alarm;
        }

        private readonly PagesManagerService _pages;
        private readonly ISearchEngine _search;
        private readonly AppDbContext _db;
        private readonly WorkerAlarmService _alarm;
        
        /// <summary>
        /// Readable captions of the fields.
        /// </summary>
        private static IDictionary<string, string> FieldCaptions = new Dictionary<string, string>
        {
            [nameof(PageEditorVM.Title)] = "Заголовок",
            [nameof(PageEditorVM.Description)] = "Текст",
            [nameof(PageEditorVM.Facts)] = "Факты",
            [nameof(PageEditorVM.Aliases)] = "Ссылки",
            [nameof(PageEditorVM.MainPhotoKey)] = "Фото",
        };

        /// <summary>
        /// Displays the list of pages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PagesListRequestVM request)
        {
            var vm = await _pages.GetPagesAsync(request);
            return View(vm);
        }

        /// <summary>
        /// Dispays the editor form for a new page.
        /// </summary>
        [HttpGet]
        [Route("create")]
        public async Task<ActionResult> Create([FromQuery]PageType type = PageType.Person)
        {
            var vm = await _pages.RequestCreateAsync(type, User);
            if (vm.Type != type)
            {
                TempData[NotificationsService.NOTE_PAGETYPE_RESET_FROM_DRAFT] = type;
                return RedirectToAction("Create", "Pages", new {area = "Admin", type = vm.Type});
            }

            return await ViewEditorFormAsync(vm, displayDraft: true);
        }

        /// <summary>
        /// Attempts to create a new page.
        /// </summary>
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create(PageEditorVM vm)
        {
            if(!ModelState.IsValid)
                return await ViewEditorFormAsync(vm);

            try
            {
                var page = await _pages.CreateAsync(vm, User);
                await _db.SaveChangesAsync();
                await _search.AddPageAsync(page);
                _alarm.FireTreeLayoutRegenerationRequired();

                return RedirectToSuccess("Страница создана");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewEditorFormAsync(vm);
            }
        }

        /// <summary>
        /// Displays the editor for an existing page.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(Guid id)
        {
            var vm = await _pages.RequestUpdateAsync(id, User);
            return await ViewEditorFormAsync(vm, displayDraft: true);
        }

        /// <summary>
        /// Attempts to update the existing page.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(PageEditorVM vm, string tab)
        {
            if(!ModelState.IsValid)
                return await ViewEditorFormAsync(vm, tab);

            try
            {
                var page = await _pages.UpdateAsync(vm, User);
                await _db.SaveChangesAsync();
                await _search.AddPageAsync(page);
                _alarm.FireTreeLayoutRegenerationRequired();

                return RedirectToSuccess("Страница обновлена");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return await ViewEditorFormAsync(vm, tab);
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
            await _search.RemovePageAsync(page);
            _alarm.FireTreeLayoutRegenerationRequired();

            return RedirectToSuccess("Страница удалена");
        }

        #region Helpers

        /// <summary>
        /// Displays the editor form.
        /// </summary>
        private async Task<ActionResult> ViewEditorFormAsync(PageEditorVM vm, string tab = null, bool displayDraft = false)
        {
            var groups = FactDefinitions.Groups[vm.Type];
            var editorTpls = groups.SelectMany(x => x.Defs)
                                   .Select(x => x.Kind)
                                   .Distinct()
                                   .Select(x => $"~/Areas/Admin/Views/Pages/Facts/{x.Name}.cshtml")
                                   .ToList();

            var errorFields = ModelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
                                        .Select(x => FieldCaptions.TryGetValue(x.Key))
                                        .JoinString(", ");

            var photoThumbUrl = await GetMainPhotoThumbnailUrlAsync(vm.MainPhotoKey);
            var draft = await _pages.GetPageDraftAsync(vm.Id, User);

            ViewBag.Data = new PageEditorDataVM
            {
                IsNew = vm.Id == Guid.Empty,
                PageTypes = ViewHelper.GetEnumSelectList(vm.Type),
                FactGroups = groups,
                EditorTemplates = editorTpls,
                Tab = StringHelper.Coalesce(tab, "main"),
                ErrorFields = errorFields,
                MainPhotoThumbnailUrl = photoThumbUrl,
                DraftId = draft?.Id,
                DraftLastUpdateDate = draft?.LastUpdateDate,
                DraftDisplayNotification = draft != null && displayDraft

            };

            return View("Editor", vm);
        }

        /// <summary>
        /// Returns the thumbnail preview for a photo.
        /// </summary>
        private async Task<string> GetMainPhotoThumbnailUrlAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var path = await _db.Media
                                .Where(x => x.Key == key && x.Type == MediaType.Photo)
                                .Select(x => x.FilePath)
                                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(path))
                return null;

            return MediaPresenterService.GetSizedMediaPath(path, MediaSize.Small);
        }

        #endregion
    }
}
