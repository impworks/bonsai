using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
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
        public DynamicConfigController(DynamicConfigManagerService configMgr, BonsaiConfigService config, AppDbContext db)
        {
            _configMgr = configMgr;
            _config = config;
            _db = db;
        }

        private readonly DynamicConfigManagerService _configMgr;
        private readonly BonsaiConfigService _config;
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
            await _configMgr.UpdateAsync(vm);
            await _db.SaveChangesAsync();

            _config.ResetCache();

            return RedirectToSuccess("Настройки сохранены");
        }
    }
}
