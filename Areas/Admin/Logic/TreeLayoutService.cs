using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Workers;
using Bonsai.Data;
using Bonsai.Data.Utils;
using Dapper;
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

        public TreeLayoutService(WorkerAlarmService alarm, IServiceProvider services, ILogger logger)
            : base(services)
        {
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
        private bool _flush;

        #endregion

        #region Processor logic

        /// <summary>
        /// Main loop.
        /// </summary>
        protected override async Task<bool> ProcessAsync(IServiceScope scope)
        {
            try
            {
                var db = scope.ServiceProvider.GetService<AppDbContext>();

                var hasPages = await db.Pages.AnyAsync(x => x.TreeLayoutId == null);
                if (!hasPages)
                    return true;

                if (_flush)
                    await FlushTreeAsync(db);

                // todo
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