using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrchardCore.HealthChecks.Services;

public interface IHealthChecksResponseWriter
{
    Task WriteResponseAsync(HttpContext context, HealthReport report);
}
