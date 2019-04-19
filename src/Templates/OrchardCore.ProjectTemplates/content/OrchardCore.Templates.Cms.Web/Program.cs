using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder
#if (UseNLog)
                        .UseNLogWeb()
#endif
#if (UseSerilog)
                        .UseSerilogWeb()
#endif
                        .UseStartup<Startup>())
                .Build();
    }
}
