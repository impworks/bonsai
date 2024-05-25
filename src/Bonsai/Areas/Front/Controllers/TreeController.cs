using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers;

/// <summary>
/// Controller for displaying trees.
/// </summary>
[Area("Front")]
[Route("tree")]
[Authorize(Policy = AuthRequirement.Name)]
public class TreeController(PagePresenterService pagesSvc, TreePresenterService treeSvc, CacheService cacheSvc)
    : AppControllerBase
{
    /// <summary>
    /// Displays the internal page for a tree.
    /// </summary>
    [Route("{key}")]
    public async Task<ActionResult> Main(string key, [FromQuery] TreeKind kind = TreeKind.FullTree)
    {
        var encKey = PageHelper.EncodeTitle(key);
        if (encKey != key)
            return RedirectToActionPermanent("Main", new { key = encKey });

        var model = await cacheSvc.GetOrAddAsync(key + "." + kind, () => pagesSvc.GetPageTreeAsync(key, kind));

        return View(model);
    }
        
    /// <summary>
    /// Returns the rendered tree.
    /// </summary>
    [Route("~/util/tree/{key}")]
    public async Task<ActionResult> GetTreeData(string key, [FromQuery] TreeKind kind = TreeKind.FullTree)
    {
        var encKey = PageHelper.EncodeTitle(key);
        var data = await treeSvc.GetTreeAsync(encKey, kind);
        return Json(data);
    }
}