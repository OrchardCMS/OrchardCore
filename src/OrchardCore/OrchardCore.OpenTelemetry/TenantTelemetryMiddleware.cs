using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Telemetry;

/// <summary>
/// Tags the span and the metrics of an HTTP request with the name of the tenant that served it, and reports the
/// database operations it ran. The tenant is only known once the request has been processed by the Orchard Core pipeline.
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
        // Counts the database operations of the request only when they're reported.
        var database = IsRecorded(context)
            ? DatabaseTelemetryScope.Begin()
            : null;

        try
        {
            await _next(context);
        }
        finally
        {
            var tenantName = context.Features.Get<ShellContextFeature>()?.ShellContext.Settings.Name;
            if (tenantName is not null)
            {
                var activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
                activity?.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);

                if (_includeTenantNameInMetrics)
                {
                    context.Features.Get<IHttpMetricsTagsFeature>()?.Tags.Add(new(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName));
                }

                if (database is not null)
                {
                    RecordDatabaseOperations(context, activity, database, tenantName);
                }
            }
        }
    }

    private static bool IsRecorded(HttpContext context)
    {
        return RequestDatabaseMetrics.Operations.Enabled ||
            RequestDatabaseMetrics.Duration.Enabled ||
            context.Features.Get<IHttpActivityFeature>()?.Activity.IsAllDataRequested == true;
    }

    private static void RecordDatabaseOperations(HttpContext context, Activity activity, DatabaseTelemetryScope database, string tenantName)
    {
        activity?.SetTag(OrchardCoreTelemetryConstants.Attributes.DbOperations, database.OperationCount);
        activity?.SetTag(OrchardCoreTelemetryConstants.Attributes.DbDuration, database.Duration.TotalSeconds);

        if (!RequestDatabaseMetrics.Operations.Enabled && !RequestDatabaseMetrics.Duration.Enabled)
        {
            return;
        }

        var tags = new TagList
        {
            { OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName },
        };

        // The route template, such as 'Admin/Contents/ContentItems/{contentItemId}/Edit', tells which endpoint runs too many queries.
        if (context.GetEndpoint() is RouteEndpoint { RoutePattern.RawText: { Length: > 0 } route })
        {
            tags.Add(OrchardCoreTelemetryConstants.Attributes.HttpRoute, route);
        }

        RequestDatabaseMetrics.Operations.Record(database.OperationCount, tags);
        RequestDatabaseMetrics.Duration.Record(database.Duration.TotalSeconds, tags);
    }
}
