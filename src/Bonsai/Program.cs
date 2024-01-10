using System.IO;
using System.Reflection;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Code.Config;
using Bonsai.Code.Services.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Bonsai
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) =>
                {
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs/bonsai-.txt");
                    config
                        .Enrich.FromLogContext()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                        .WriteTo.Console()
                        .WriteTo.Debug()
                        .WriteTo.File(path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
                })
                .ConfigureWebHostDefaults(web =>
                {
                    web.UseKestrel()
                       .UseUrls("http://0.0.0.0:80/")
                       .UseContentRoot(Directory.GetCurrentDirectory())
                       .UseIIS()
                       .UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<BackgroundJobService>();
                    services.AddSingleton<IBackgroundJobService>(sp => sp.GetService<BackgroundJobService>());
                    services.AddHostedService(sp => sp.GetService<BackgroundJobService>());

                    services.AddTransient<MediaEncoderJob>();
                    services.AddTransient<TreeLayoutJob>();
                })
                .Build()
                .Run();
        }
    }
}
