using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using System;
using System.IO;

namespace Orchard.Hosting.CommandConsole {
    public class Program {
        private const string HostingIniFile = "Microsoft.AspNet.Hosting.ini";
        private const string ConfigFileKey = "config";

        private readonly IServiceProvider _serviceProvider;

        public Program(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        //https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNet.Hosting/Program.cs
        public void Main(string[] args) {
            // Allow the location of the ini file to be specified via a --config command line arg
            var tempBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var tempConfig = tempBuilder.Build();
            var configFilePath = tempConfig[ConfigFileKey] ?? HostingIniFile;

            var appBasePath = _serviceProvider.GetRequiredService<IApplicationEnvironment>().ApplicationBasePath;
            var builder = new ConfigurationBuilder(appBasePath);
            builder.AddIniFile(configFilePath, optional: true);
            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);
            var config = builder.Build();

            var host = new WebHostBuilder(_serviceProvider, config).Build();
            using (host.Start()) {
                Console.WriteLine("Application started. Press Ctrl+C to shut down.");

                var appShutdownService = host.ApplicationServices.GetRequiredService<IApplicationShutdown>();
                Console.CancelKeyPress += (sender, eventArgs) => {
                    appShutdownService.RequestShutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };
                appShutdownService.ShutdownRequested.WaitHandle.WaitOne();

                Console.SetIn(new StreamReader(Console.OpenStandardInput()));
                //return (int)new OrchardHost(Console.In, Console.Out, args).Run();
            }
        }
    }
}
