using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Areas.Admin.ViewModels.DynamicConfig;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Code.Services;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing the global configuration.
    /// </summary>
    [Route("admin/config")]
    public class DynamicConfigController: AdminControllerBase
    {
        public DynamicConfigController(DynamicConfigManagerService configMgr, BonsaiConfigService config, IBackgroundJobService jobs, AppDbContext db, CacheService cache)
        {
            _configMgr = configMgr;
            _config = config;
            _jobs = jobs;
            _db = db;
            _cache = cache;
        }

        private readonly DynamicConfigManagerService _configMgr;
        private readonly BonsaiConfigService _config;
        private readonly IBackgroundJobService _jobs;
        private readonly AppDbContext _db;
        private readonly CacheService _cache;

        /// <summary>
        /// Displays the form and edits the config.
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var vm = await _configMgr.RequestUpdateAsync();
            return View(vm);
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> Update(UpdateDynamicConfigVM vm)
        {
            var oldValue = await _configMgr.RequestUpdateAsync();
            await _configMgr.UpdateAsync(vm);
            await _db.SaveChangesAsync();

            _config.ResetCache();

            if (IsTreeConfigChanged())
            {
                _cache.Remove<PageTreeVM>();
                await _jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());
            }

            return RedirectToSuccess("Настройки сохранены");

            bool IsTreeConfigChanged()
            {
                return oldValue.TreeRenderThoroughness != vm.TreeRenderThoroughness
                       || oldValue.TreeKinds?.JoinString(",") != vm.TreeKinds?.JoinString(",");
            }
        }
    }
}
