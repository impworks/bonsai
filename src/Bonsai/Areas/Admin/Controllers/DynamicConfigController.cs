using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Areas.Admin.ViewModels.DynamicConfig;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Bonsai.Localization;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers;

/// <summary>
/// Controller for managing the global configuration.
/// </summary>
[Route("admin/config")]
public class DynamicConfigController(DynamicConfigManagerService configMgr, BonsaiConfigService configSvc, IBackgroundJobService jobsScv, AppDbContext db, CacheService cache)
    : AdminControllerBase
{
    /// <summary>
    /// Displays the form and edits the config.
    /// </summary>
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> Index()
    {
        var vm = await configMgr.RequestUpdateAsync();
        return View(vm);
    }

    /// <summary>
    /// Updates the settings.
    /// </summary>
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> Update(UpdateDynamicConfigVM vm)
    {
        var oldValue = await configMgr.RequestUpdateAsync();
        await configMgr.UpdateAsync(vm);
        await db.SaveChangesAsync();

        configSvc.ResetCache();

        if (IsTreeConfigChanged())
        {
            cache.Remove<PageTreeVM>();
            await jobsScv.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());
        }

        return RedirectToSuccess(Texts.Admin_Config_SavedMessage);

        bool IsTreeConfigChanged()
        {
            return oldValue.TreeRenderThoroughness != vm.TreeRenderThoroughness
                   || oldValue.TreeKinds?.JoinString(",") != vm.TreeKinds?.JoinString(",");
        }
    }
}