using Microsoft.AspNetCore.Hosting;
using Orchard.Hosting;

namespace Orchard.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseDefaultHostingConfiguration(args)
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