using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Mvc.Web
{
    public class Program
    {
        public static void Main(string[] args)
            => BuildHost(args).Run();

        public static IHost BuildHost(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>())
                .Build();
    }
}
