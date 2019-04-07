using System.IO;
using Bonsai.Code.Config;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Bonsai
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                       .UseKestrel()
                       .UseContentRoot(Directory.GetCurrentDirectory())
                       .UseIISIntegration()
                       .UseSerilog((context, config) =>
                       {
                           config
                               .Enrich.FromLogContext()
                               .MinimumLevel.Information()
                               .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                               .WriteTo.Console()
                               .WriteTo.File("Logs/workers-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
                       })
                       .UseStartup<Startup>()
                       .Build();

                host.Run();
        }
    }
}
