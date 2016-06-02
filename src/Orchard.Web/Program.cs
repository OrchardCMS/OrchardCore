using System.IO;
using Microsoft.AspNetCore.Hosting;
using Orchard.Hosting;
using Orchard.Web;

namespace Orchard.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var host = new WebHostBuilder()
                .UseIISIntegration()
                .UseKestrel()
                .UseContentRoot(currentDirectory)
                .UseWebRoot(currentDirectory)
                .UseStartup<Startup>()
                .Build();

            using (host)
            {
                host.Start();

                var orchardHost = new OrchardHost(host.Services, System.Console.In, System.Console.Out, args);
                orchardHost.Run();
            }
        }
    }
}