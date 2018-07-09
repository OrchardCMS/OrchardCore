using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace OrchardCore.Logging
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSerilogWeb(this IWebHostBuilder builder)
        {
            return builder.UseSerilog((hostingContext, configBuilder) =>
            {
                configBuilder.WriteTo.Console()
                .MinimumLevel.Verbose()
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext();
            });
        }
    }
}
