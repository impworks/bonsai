using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Areas.Admin.ViewModels.DynamicConfig;
using Bonsai.Code.Services.Config;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing the global configuration.
    /// </summary>
    [Route("admin/config")]
    public class DynamicConfigController: AdminControllerBase
    {
        public DynamicConfigController(DynamicConfigManagerService configMgr, BonsaiConfigService config, WorkerAlarmService alarm, AppDbContext db)
        {
            _configMgr = configMgr;
            _config = config;
            _alarm = alarm;
            _db = db;
        }

        private readonly DynamicConfigManagerService _configMgr;
        private readonly BonsaiConfigService _config;
        private readonly WorkerAlarmService _alarm;
        private readonly AppDbContext _db;

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

            if(oldValue.TreeRenderThoroughness != vm.TreeRenderThoroughness)
                _alarm.FireTreeLayoutRegenerationRequired();

            return RedirectToSuccess("Настройки сохранены");
        }
    }
}
