using System.IO;
using Microsoft.AspNetCore.Hosting;
using Orchard.Hosting;
using Orchard.Web;
using System.Threading;

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
                using (var cts = new CancellationTokenSource())
                {
                    host.Run((services) =>
                    {
                        var orchardHost = new OrchardHost(
                            services, 
                            System.Console.In, 
                            System.Console.Out, 
                            args);

                        orchardHost
                            .RunAsync()
                            .Wait();

                        cts.Cancel();

                    }, cts.Token, "Application started. Press Ctrl+C to shut down.");
                }
            }
        }
    }
}