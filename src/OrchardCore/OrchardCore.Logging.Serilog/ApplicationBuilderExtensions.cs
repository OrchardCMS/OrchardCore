using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Hosting.ShellBuilders;
using Serilog;
using Serilog.Context;

namespace OrchardCore.Logging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSerilogTenantNameLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogTenantNameLoggingMiddleware>();
        }
    }
}
