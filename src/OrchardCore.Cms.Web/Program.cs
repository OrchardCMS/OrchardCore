using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OrchardCore.Logging;

namespace OrchardCore.Cms.Web
{
    public class Program
    {
        public static void Main(string[] args)
            => BuildHost(args).Run();

        public static IHost BuildHost(string[] args)
            => CreateHostBuilder(args)
                .Build();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>()
                        .UseNLogWeb());
    }
}