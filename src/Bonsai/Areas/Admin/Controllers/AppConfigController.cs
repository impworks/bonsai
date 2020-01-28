using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Config;
using Bonsai.Code.Services.Config;
using Bonsai.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing the global configuration.
    /// </summary>
    [Route("admin/config")]
    public class AppConfigController: AdminControllerBase
    {
        public AppConfigController(AppConfigManagerService configMgr, AppConfigService config, AppDbContext db)
        {
            _configMgr = configMgr;
            _config = config;
            _db = db;
        }

        private readonly AppConfigManagerService _configMgr;
        private readonly AppConfigService _config;
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
        public async Task<ActionResult> Update(UpdateAppConfigVM vm)
        {
            await _configMgr.UpdateAsync(vm);
            await _db.SaveChangesAsync();

            _config.ResetCache();

            return RedirectToSuccess("Настройки сохранены");
        }
    }
}
