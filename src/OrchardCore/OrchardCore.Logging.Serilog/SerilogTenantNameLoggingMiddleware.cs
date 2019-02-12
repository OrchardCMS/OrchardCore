using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell.Builders;
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
