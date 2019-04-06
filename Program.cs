using System.IO;
using Bonsai.Code.Config;
using Microsoft.AspNetCore.Hosting;
using Serilog;

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
                               .WriteTo.File("bonsai-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
                       })
                       .UseStartup<Startup>()
                       .Build();

                host.Run();
        }
    }
}
