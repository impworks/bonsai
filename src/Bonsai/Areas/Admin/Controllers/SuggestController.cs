using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers;

/// <summary>
/// Controller for ajax lookups.
/// </summary>
[Route("admin")]
public class SuggestController(SuggestService suggestSvc) : AdminControllerBase
{
    /// <summary>
    /// Suggests pages for relation destination / media tag.
    /// </summary>
    [HttpGet]
    [Route("suggest/pages")]
    public async Task<ActionResult> SuggestPages([FromQuery] PickRequestVM<PageType> vm)
    {
        var pages = await suggestSvc.SuggestPagesAsync(vm);
        return Json(pages);
    }

    /// <summary>
    /// Suggests pages for relation source.
    /// </summary>
    [HttpGet]
    [Route("suggest/pages/rel")]
    public async Task<ActionResult> SuggestPagesForRelationSource([FromQuery] RelationSuggestQueryVM vm)
    {
        var pages = await suggestSvc.SuggestRelationPagesAsync(vm);
        return Json(pages);
    }

    /// <summary>
    /// Returns data for page picker.
    /// </summary>
    [HttpGet]
    [Route("pick/pages")]
    public async Task<ActionResult> PickPages([FromQuery] PickRequestVM<PageType> vm)
    {
        var media = await suggestSvc.GetPickablePagesAsync(vm);
        return Json(media);
    }

    /// <summary>
    /// Returns data for media picker.
    /// </summary>
    [HttpGet]
    [Route("pick/media")]
    public async Task<ActionResult> PickMedia([FromQuery] PickRequestVM<MediaType> vm)
    {
        var media = await suggestSvc.GetPickableMediaAsync(vm);
        return Json(media);
    }
}