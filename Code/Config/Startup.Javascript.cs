using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.Jint;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers Automapper.
        /// </summary>
        private void ConfigureJavascriptEngine(IServiceCollection services)
        {
            services.AddJsEngineSwitcher(opts => opts.DefaultEngineName = JintJsEngine.EngineName)
                    .AddJint(j => j.StrictMode = true);
        }
    }
}
