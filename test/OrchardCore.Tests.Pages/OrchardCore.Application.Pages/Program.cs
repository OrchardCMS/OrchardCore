using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Application.Pages
{
    public class Program
    {
        public static Task Main(string[] args)
            => BuildHost(args).RunAsync();

        public static IHost BuildHost(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>())
                .Build();
    }
}
