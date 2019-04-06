using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers JS execution engine.
        /// </summary>
        private void ConfigureJavascriptEngine(IServiceCollection services)
        {
            services.AddJsEngineSwitcher(opts => opts.DefaultEngineName = ChakraCoreJsEngine.EngineName)
                    .AddChakraCore();
        }
    }
}
