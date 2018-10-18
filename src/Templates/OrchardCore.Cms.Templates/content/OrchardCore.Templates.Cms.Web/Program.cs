using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#if (UseNLog || UseSerilog)
using OrchardCore.Logging;
#endif
using OrchardCore.Modules;

namespace OrchardCore.Templates.Cms.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if (UseNLog)
                .UseNLogWeb()
#endif
#if (UseSerilog)
                .UseSerilogWeb()
#endif
                .UseStartup<Startup>()
                .Build();
    }
}
