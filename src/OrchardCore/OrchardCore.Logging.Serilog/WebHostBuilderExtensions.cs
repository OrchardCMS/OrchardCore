using System;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace OrchardCore.Logging
{
    public static class WebHostBuilderExtensions
    {
        [Obsolete]
        public static IWebHostBuilder UseSerilogWeb(this IWebHostBuilder builder)
        {
            return builder.UseSerilog((hostingContext, configBuilder) =>
            {
                configBuilder.ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext();
            });
        }
    }
}
