using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers;

/// <summary>
/// Controller for handling drafts.
/// </summary>
[Route("admin/drafts")]
public class DraftsController(PagesManagerService pagesSvc, PagePresenterService pagePresenter, AppDbContext db)
    : AdminControllerBase
{
    /// <summary>
    /// Discards the draft of a page.
    /// </summary>
    [HttpPost]
    [Route("update")]
    public async Task<ActionResult> Update(PageEditorVM vm)
    {
        var info = await pagesSvc.UpdatePageDraftAsync(vm, User);
        await db.SaveChangesAsync();

        return Json(info);
    }

    /// <summary>
    /// Discards the draft of a page.
    /// </summary>
    [HttpPost]
    [Route("remove")]
    public async Task<ActionResult> Remove(Guid? id, PageType? type)
    {
        await pagesSvc.DiscardPageDraftAsync(id, User);
        await db.SaveChangesAsync();

        if (id != null && id != Guid.Empty)
            return RedirectToAction("Update", "Pages", new { id = id });

        return type == null
            ? RedirectToAction("Index", "Pages")
            : RedirectToAction("Create", "Pages", new { type = type });
    }

    [HttpGet]
    [Route("preview")]
    public async Task<ActionResult> Preview(Guid? id)
    {
        var page = await pagesSvc.GetPageDraftPreviewAsync(id, User);
        if (page == null)
            return NotFound();

        var vm = new PageVM<PageDescriptionVM>
        {
            Body = await pagePresenter.GetPageDescriptionAsync(page),
            InfoBlock = await pagePresenter.GetPageInfoBlockAsync(page)
        };
        ViewBag.IsPreview = true;
        ViewBag.DisableSearch = true;
        return View("~/Areas/Front/Views/Page/Description.cshtml", vm);
    }
}