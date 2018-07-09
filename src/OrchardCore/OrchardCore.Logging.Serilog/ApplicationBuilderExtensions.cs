using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Hosting.ShellBuilders;
using Serilog;
using Serilog.Context;

namespace OrchardCore.Logging.Serilog
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSerilogWeb(this IApplicationBuilder builder)
        {
            builder.Use(async (ctx, next) => {
                // If there is no ShellContext in the Features then the log is rendered from the Host
                using (LogContext.PushProperty("TenantName", ctx.Features.Get<ShellContext>()?.Settings?.Name ?? "None"))
                {
                    await next();
                }
            });
            return builder;
        }
    }
}
