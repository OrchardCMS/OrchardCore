using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Decides, before Orchard Core runs a tenant, whether this instance forwards the request to the cluster of its tenant.
/// A forwarded request doesn't enter the rest of the pipeline, so no tenant is built on a clusters proxy.
/// </summary>
public sealed class ClustersRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IShellHost _shellHost;
    private readonly IRunningShellTable _runningShellTable;
    private readonly IOptionsMonitor<ClustersOptions> _optionsMonitor;

    public ClustersRoutingMiddleware(
        RequestDelegate next,
        IShellHost shellHost,
        IRunningShellTable runningShellTable,
        IOptionsMonitor<ClustersOptions> optionsMonitor)
    {
        _next = next;
        _shellHost = shellHost;
        _runningShellTable = runningShellTable;
        _optionsMonitor = optionsMonitor;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var options = _optionsMonitor.CurrentValue;

        if (!httpContext.AsClustersProxy(options))
        {
            // The proxy secret should not be readable by the code running in a tenant.
            httpContext.Request.Headers.Remove(RequestHeaderNames.FromClustersProxy);

            await _next(httpContext);
            return;
        }

        var endpoint = httpContext.GetEndpoint();

        // An endpoint more specific than the route template, mapped by the host application, is not forwarded.
        if (endpoint is not null && !ClustersProxyMatcherPolicy.IsRouteTemplate(endpoint))
        {
            await _next(httpContext);
            return;
        }

        // Ensure all ShellContext are loaded and available.
        await _shellHost.InitializeAsync();

        var shellSettings = _runningShellTable.Match(httpContext.Request.Host, httpContext.Request.Path, true);

        if (shellSettings is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var clusterId = shellSettings.GetClusterId(options);

        if (clusterId is null || endpoint is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await httpContext.Response.WriteAsync("No cluster is configured for the requested tenant.", httpContext.RequestAborted);
            return;
        }

        httpContext.Features.Set(new ClusterFeature
        {
            ClusterId = clusterId,
        });

        await endpoint.RequestDelegate(httpContext);
    }
}
