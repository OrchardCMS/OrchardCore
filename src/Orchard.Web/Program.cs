using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Orchard.Hosting.Console;
using System;
using System.Threading.Tasks;

namespace Orchard.Console {
    public class Program {
        private const string HostingIniFile = "Microsoft.AspNet.Hosting.ini";
        private const string ConfigFileKey = "config";

        private readonly IServiceProvider _serviceProvider;

        public Program(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        //https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNet.Hosting/Program.cs
        public Task<int> Main(string[] args) {
            //Add command line configuration source to read command line parameters.
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            var webhost = new WebHostBuilder(_serviceProvider, config)
                .UseServer("Microsoft.AspNet.Server.WebListener")
                .Build();
            using (var host = webhost.Start()) {
                var orchardHost = new OrchardHost(webhost.ApplicationServices, System.Console.In, System.Console.Out, args);

                return Task.FromResult(
                    (int)orchardHost.Run());
            }
        }
    }
}
