using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy;

namespace OrchardCore.Clusters;

/// <summary>
/// Distributes proxy requests across tenant clusters.
/// </summary>
public sealed class ClustersProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _lookup;

    public ClustersProxyMiddleware(RequestDelegate next, IProxyStateLookup proxyStateLookup)
    {
        _next = next;
        _lookup = proxyStateLookup;
    }

    public async Task Invoke(HttpContext context)
    {
        // A tenant should only be served by its cluster, so don't let the route forward it to its default cluster.
        var clusterId = context.Features.Get<ClusterFeature>()?.ClusterId;

        if (clusterId is null || !_lookup.TryGetCluster(clusterId, out var cluster))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        // Distribute the proxy request to this tenant cluster.
        context.ReassignProxyRequest(cluster);

        await _next(context);
    }
}
