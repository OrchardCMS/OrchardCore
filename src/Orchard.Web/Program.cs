using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Orchard.Hosting;
using System;
using System.Threading.Tasks;

namespace Orchard.Console {
    public class Program {
        private readonly IServiceProvider _serviceProvider;

        public Program(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        //https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNet.Hosting/Program.cs
        public Task<int> Main(string[] args) {
            //Add command line configuration source to read command line parameters.
            var builder = new ConfigurationBuilder().AddCommandLine(args);
            var config = builder.Build();

            var host = new WebHostBuilder(_serviceProvider, config, true)
                .UseServer("Microsoft.AspNet.Server.Kestrel")
                .Build();
            using (var app = host.Start()) {
                var orchardHost = new OrchardHost(app.Services, System.Console.In, System.Console.Out, args);

                return Task.FromResult(
                    (int)orchardHost.Run());
            }
        }
    }
}
