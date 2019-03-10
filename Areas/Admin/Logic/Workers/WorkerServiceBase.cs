using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bonsai.Areas.Admin.Logic.Workers
{
    /// <summary>
    /// Base class for background worker services.
    /// </summary>
    public abstract class WorkerServiceBase: IHostedService
    {
        #region Constructor

        protected WorkerServiceBase(IServiceProvider services)
        {
            _services = services;
            _cancellationSource = new CancellationTokenSource();
        }

        #endregion

        #region Fields

        private readonly IServiceProvider _services;
        protected readonly CancellationTokenSource _cancellationSource;
        protected CancellationToken Token => _cancellationSource.Token;

        private Thread _thread;
        protected bool _isAsleep;

        #endregion

        #region IHostedService implementation

        /// <summary>
        /// Starts the background encoder service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _thread = new Thread(MainLoop) { IsBackground = true };
            _thread.Start();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationSource.Cancel();

            return Task.CompletedTask;
        }

        #endregion

        #region Worker logic

        /// <summary>
        /// Sync wrapper for the main loop.
        /// </summary>
        private void MainLoop()
        {
            MainLoopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Main loop.
        /// </summary>
        private async Task MainLoopAsync()
        {
            using (var scope = _services.CreateScope())
                await InitializeAsync(scope.ServiceProvider);

            while (true)
            {
                if (_isAsleep)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), Token);
                    continue;
                }

                using(var scope = _services.CreateScope())
                    _isAsleep = await ProcessAsync(scope.ServiceProvider);
            }
        }

        /// <summary>
        /// Handles a request if the service is not asleep.
        /// </summary>
        protected abstract Task<bool> ProcessAsync(IServiceProvider scope);

        /// <summary>
        /// Performs initialization if required.
        /// </summary>
        protected virtual async Task InitializeAsync(IServiceProvider services)
        {
            // does nothing in base
        }

        #endregion
    }
}
