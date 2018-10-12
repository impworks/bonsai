using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for handling drafts.
    /// </summary>
    [Route("admin/drafts")]
    public class DraftsController: AdminControllerBase
    {
        public DraftsController(PagesManagerService pages, PagePresenterService pagePresenter, AppDbContext db)
        {
            _pages = pages;
            _pagePresenter = pagePresenter;
            _db = db;
        }

        private readonly PagesManagerService _pages;
        private readonly PagePresenterService _pagePresenter;
        private readonly AppDbContext _db;
        
        /// <summary>
        /// Discards the draft of a page.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(PageEditorVM vm)
        {
            var info = await _pages.UpdatePageDraftAsync(vm, User);
            await _db.SaveChangesAsync();

            return Json(info);
        }
        
        /// <summary>
        /// Discards the draft of a page.
        /// </summary>
        [HttpPost]
        [Route("remove")]
        public async Task<ActionResult> Remove(Guid? id)
        {
            await _pages.DiscardPageDraftAsync(id, User);
            await _db.SaveChangesAsync();

            if (id == null || id == Guid.Empty)
                return RedirectToAction("Index", "Pages");

            return RedirectToAction("Update", "Pages", new {id = id});
        }

        [HttpGet]
        [Route("preview")]
        public async Task<ActionResult> Preview(Guid? id)
        {
            var page = await _pages.GetPageDraftPreviewAsync(id, User);
            if (page == null)
                return NotFound();

            var vm = await _pagePresenter.GetPageDescriptionAsync(page);
            ViewBag.IsPreview = true;
            return View("~/Areas/Front/Views/Page/Description.cshtml", vm);
        }
    }
}
