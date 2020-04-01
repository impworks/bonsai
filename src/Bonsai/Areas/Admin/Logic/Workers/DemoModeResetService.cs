using System;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Microsoft.Extensions.Hosting;

namespace Bonsai.Areas.Admin.Logic.Workers
{
    /// <summary>
    /// Service for automatically restarting the demo instance.
    /// </summary>
    public class DemoModeResetService: IHostedService
    {
        public DemoModeResetService(BonsaiConfigService config)
        {
            _demoCfg = config.GetStaticConfig().DemoMode;
        }

        private readonly DemoModeConfig _demoCfg;

        /// <summary>
        /// Waits for the specified time and terminates the app.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_demoCfg.Enabled || _demoCfg.ResetInterval.TotalSeconds < 1)
                return;

            // sic! runs in background
            Task.Run(async () =>
            {
                await Task.Delay(_demoCfg.ResetInterval, cancellationToken);
                Environment.Exit(0);
            });
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
