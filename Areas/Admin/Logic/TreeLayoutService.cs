using System;
using System.IO;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Data;
using Bonsai.Data.Utils;
using Dapper;
using JavaScriptEngineSwitcher.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// A background service for calculating family tree layouts.
    /// </summary>
    public class TreeLayoutService: WorkerServiceBase
    {
        #region Constructor

        public TreeLayoutService(WorkerAlarmService alarm, IServiceProvider services, IHostingEnvironment env, ILogger logger)
            : base(services)
        {
            _env = env;
            _logger = logger;

            alarm.OnTreeLayoutRegenerationRequired += (s, e) =>
            {
                _isAsleep = false;
                _flush = true;
            };
        }

        #endregion

        #region Fields

        private readonly ILogger _logger;
        private readonly IHostingEnvironment _env;
        private bool _flush;

        private IPrecompiledScript _jsScript;

        #endregion

        #region Processor logic

        protected override async Task InitializeAsync(IServiceProvider services)
        {
            using (var js = services.GetService<IJsEngineSwitcher>().CreateDefaultEngine())
            {
                var filePath = Path.Combine(_env.ContentRootPath, "assets", "scripts", "tree.js");
                var fileContents = File.ReadAllText(filePath);
                _jsScript = js.Precompile(fileContents);
            }
        }

        /// <summary>
        /// Main loop.
        /// </summary>
        protected override async Task<bool> ProcessAsync(IServiceProvider services)
        {
            try
            {
                using (var db = services.GetService<AppDbContext>())
                using (var js = services.GetService<IJsEngineSwitcher>().CreateDefaultEngine())
                {

                    var hasPages = await db.Pages.AnyAsync(x => x.TreeLayoutId == null);
                    if (!hasPages)
                        return true;

                    if (_flush)
                        await FlushTreeAsync(db);

                    // todo
                }
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException))
                    _logger.Error(ex, "Failed to generate a tree layout.");
            }

            return false;
        }

        #endregion

        #region Tree generation

        /// <summary>
        /// Removes all existing tree layouts.
        /// </summary>
        private async Task FlushTreeAsync(AppDbContext db)
        {
            using (var conn = db.GetConnection())
            {
                await conn.ExecuteAsync(@"UPDATE ""Pages"" SET TreeLayoutId = NULL");
                await conn.ExecuteAsync(@"DELETE FROM ""TreeLayouts""");
            }
        } 

        #endregion
    }
}