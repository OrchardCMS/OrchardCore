using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using OrchardCore.HealthChecks.Models;

namespace OrchardCore.HealthChecks.Services;

public class DefaultHealthChecksResponseWriter : IHealthChecksResponseWriter
{
    public async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        var response = new HealthCheckReponse
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

        await context.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented));
    }
}
