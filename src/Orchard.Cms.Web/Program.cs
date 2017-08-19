using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orchard.Hosting;
using Orchard.Logging;

namespace Orchard.Cms.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddJsonFile("logging.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"logging.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                            config.AddUserSecrets(appAssembly, optional: true);
                    }

                    config.AddEnvironmentVariables();

                    if (args?.Any() ?? false) config.AddCommandLine(args);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddNLogWeb(hostingContext.HostingEnvironment);
                })
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