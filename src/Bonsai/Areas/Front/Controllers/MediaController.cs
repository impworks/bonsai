using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Front.Logic.Auth;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers;

/// <summary>
/// The controller for displaying media information.
/// </summary>
[Area("Front")]
[Route("m")]
[Authorize(Policy = AuthRequirement.Name)]
public class MediaController(MediaPresenterService mediaSvc, CacheService cacheSvc, AuthService authSvc)
    : AppControllerBase
{
    /// <summary>
    /// Displays media and details.
    /// </summary>
    [Route("{key}")]
    public async Task<ActionResult> ViewMedia(string key)
    {
        var vm = await cacheSvc.GetOrAddAsync(key, async() => await mediaSvc.GetMediaAsync(key));
        ViewBag.User = await authSvc.GetCurrentUserAsync(User);
        return View(vm);
    }
}