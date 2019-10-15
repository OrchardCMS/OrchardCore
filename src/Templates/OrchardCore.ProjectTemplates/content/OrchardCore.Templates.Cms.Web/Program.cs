using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if (UseNLog || UseSerilog)
using OrchardCore.Logging;
#endif

namespace OrchardCore.Templates.Cms.Web
{
    public class Program
    {
        public static Task Main(string[] args)
            => BuildHost(args).RunAsync();

        public static IHost BuildHost(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureWebHostDefaults(webBuilder => webBuilder
#if (UseNLog)
                        .UseNLogWeb()
#endif
#if (UseSerilog)
                        .UseSerilogWeb()
#endif
                        .UseStartup<Startup>())
                .Build();

            return host;
        }
    }
}
