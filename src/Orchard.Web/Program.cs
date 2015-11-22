using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Orchard.Hosting;
using System.Threading.Tasks;

namespace Orchard.Console
{
    public class Program
    {
        //https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNet.Hosting/Program.cs
        public Task<int> Main(string[] args)
        {
            //Add command line configuration source to read command line parameters.
            var builder = new ConfigurationBuilder().AddCommandLine(args);
            var config = builder.Build();

            var host = new WebHostBuilder(config, true)
                .UseServer("Microsoft.AspNet.Server.Kestrel")
                .Build();
            using (var app = host.Start())
            {
                var orchardHost = new OrchardHost(app.Services, System.Console.In, System.Console.Out, args);

                return Task.FromResult(
                    (int)orchardHost.Run());
            }
        }
    }
}