using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if (UseNLog || UseSerilog)
using OrchardCore.Logging;
#endif

namespace OrchardCore.Templates.Cms.Web
{
    public class Program
    {
#if (UseNLog)
        public static Task Main(string[] args)
            => BuildHost(args).RunAsync();
#else
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Orchard starting up");
                await BuildHost(args).RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Orchard start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
#endif
        public static IHost BuildHost(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
#if (UseSerilog)
                .UseSerilog()
#endif
                .ConfigureWebHostDefaults(webBuilder => webBuilder
#if (UseNLog)
                        .UseNLogWeb()
#endif
#if (UseSerilog)
                        .UseSerilogWeb()
#endif
                        .UseStartup<Startup>())
                .Build();

            return host;
        }
    }
}
