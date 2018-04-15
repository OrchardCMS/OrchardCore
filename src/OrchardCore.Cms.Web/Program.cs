using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OrchardCore.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Cms.Web
{
    public class Program
    {
        public static void Main(string[] args)
            => BuildWebHost(args).Run();

        public static IWebHost BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseKestrel(c => c.AddServerHeader = false)
                .UseNLogWeb()
                .UseStartup<Startup>()
                .Build();
    }
}