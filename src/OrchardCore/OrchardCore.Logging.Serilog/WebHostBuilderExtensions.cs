using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace OrchardCore.Logging
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSerilogWeb(this IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var env = context.HostingEnvironment;

                (configBuilder as ConfigurationBuilder).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                var config = configBuilder.Build();
                
                Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "/logs/{Date}.txt"),
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] [{EventId}] [{HttpContext.TenantName}] {Message}{NewLine}{Exception}")
                .ReadFrom.Configuration(config)
                .CreateLogger();
            });

            return builder;
        }
    }
}
