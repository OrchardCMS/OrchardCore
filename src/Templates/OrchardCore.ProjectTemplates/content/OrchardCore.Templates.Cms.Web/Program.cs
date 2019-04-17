using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
#if (UseNLog || UseSerilog)
using OrchardCore.Logging;
#endif
using OrchardCore.Modules;

namespace OrchardCore.Templates.Cms.Web
{
    public class Program
    {
        public static void Main(string[] args)
            => BuildHost(args).Run();

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
