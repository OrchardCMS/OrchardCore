using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.HealthChecks.Models;

namespace OrchardCore.HealthChecks.Services;

public class DefaultHealthChecksResponseWriter : IHealthChecksResponseWriter
{
    public async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            HealthChecks = report.Entries.Select(item => new HealthCheckEntry
            {
                Name = item.Key,
                Status = item.Value.Status.ToString(),
                Description = item.Value.Description
            })
        };

        context.Response.ContentType = MediaTypeNames.Application.Json;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
