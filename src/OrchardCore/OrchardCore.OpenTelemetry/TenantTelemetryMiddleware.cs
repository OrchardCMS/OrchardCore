using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Telemetry;

/// <summary>
/// Tags the span and the metrics of an HTTP request with the name of the tenant that served it.
/// The tenant is only known once the request has been processed by the Orchard Core pipeline.
/// </summary>
internal sealed class TenantTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _includeTenantNameInMetrics;

    public TenantTelemetryMiddleware(RequestDelegate next, IOptions<OrchardCoreOpenTelemetryOptions> options)
    {
        _next = next;
        _includeTenantNameInMetrics = options.Value.Metrics.IncludeTenantName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            var tenantName = context.Features.Get<ShellContextFeature>()?.ShellContext.Settings.Name;
            if (tenantName is not null)
            {
                context.Features.Get<IHttpActivityFeature>()?.Activity.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);

                if (_includeTenantNameInMetrics)
                {
                    context.Features.Get<IHttpMetricsTagsFeature>()?.Tags.Add(new(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName));
                }
            }
        }
    }
}
