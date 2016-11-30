using System;
using System.IO;
using DasMulli.Win32.ServiceUtils;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Orchard.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Orchard.Cms.Web
{
    public class OrchardCmsWin32Service : IWin32Service
    {
        private readonly string[] _commandLineArguments;
        private readonly bool _useInteractiveCommandLine;

        private bool _stopRequestedByWindows;
        private IWebHost _host;
        
        public OrchardCmsWin32Service(string[] commandLineArguments, bool useInteractiveCommandLine)
        {
            _commandLineArguments = commandLineArguments;
            _useInteractiveCommandLine = useInteractiveCommandLine;
        }

        public string ServiceName => "Orchard Service";

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            string[] combinedArguments;
            if (startupArguments.Length > 0)
            {
                combinedArguments = new string[_commandLineArguments.Length + startupArguments.Length];
                Array.Copy(_commandLineArguments, combinedArguments, _commandLineArguments.Length);
                Array.Copy(startupArguments, 0, combinedArguments, _commandLineArguments.Length, startupArguments.Length);
            }
            else
            {
                combinedArguments = _commandLineArguments;
            }

            var config = new ConfigurationBuilder()
                .AddCommandLine(combinedArguments)
                .Build();

            _host = new WebHostBuilder()
                .UseIISIntegration()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseConfiguration(config)
                .Build();

            if (_useInteractiveCommandLine)
            {
                using (var cts = new CancellationTokenSource())
                {
                    _host.Run((services) =>
                    {
                        var orchardHost = new OrchardHost(
                            services,
                            System.Console.In,
                            System.Console.Out,
                            combinedArguments);

                        orchardHost
                            .RunAsync()
                            .Wait();
                        cts.Cancel();
                    }, cts.Token, "Application started. Press Ctrl+C to shut down.");
                    return;
                }
            }
            else
            {
                // Make sure the windows service is stopped if the
                // ASP.NET Core stack stops for any reason
                _host.Services
                .GetRequiredService<IApplicationLifetime>()
                .ApplicationStopped
                .Register(() =>
                {
                    if (_stopRequestedByWindows == false)
                    {
                        serviceStoppedCallback();
                    }
                });

                _host.Start();
            }
        }
        public void Stop()
        {
            _stopRequestedByWindows = true;
            _host.Dispose();
        }
    }
}