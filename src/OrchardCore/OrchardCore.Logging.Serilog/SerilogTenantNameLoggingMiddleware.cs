using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Hosting.ShellBuilders;
using Serilog.Context;

namespace OrchardCore.Logging
{
    public class SerilogTenantNameLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogTenantNameLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string tenantName = context.Features.Get<ShellContext>()?.Settings?.Name ?? "None";
            using (LogContext.PushProperty("TenantName", tenantName))
            {
                await _next.Invoke(context);
            }
        }
    }
}
