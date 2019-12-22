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

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
#if (UseSerilog)
                .UseSerilog()
#endif
                .ConfigureWebHostDefaults(webBuilder => webBuilder
#if (UseNLog)
                    .UseNLogWeb()
#endif
                    .UseStartup<Startup>()
                ).Build();
    }
}
