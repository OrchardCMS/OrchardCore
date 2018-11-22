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
            => CreateWebHostBuilder(args)
                .Build();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder<Startup>(args)
                .UseNLogWeb();
    }
}