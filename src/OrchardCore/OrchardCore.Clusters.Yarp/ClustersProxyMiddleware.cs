using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy;

namespace OrchardCore.Clusters;

/// <summary>
/// Distributes proxy requests across tenant clusters.
/// </summary>
public class ClustersProxyMiddleware
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
        // Check if this instance is not used as a clusters proxy.
        if (!context.TryGetClusterFeature(out var feature) || feature.ClusterId is null)
        {
            // Bypass the clusters proxy middleware.
            await _next(context);
            return;
        }

        // Try to get the configured cluster with the same identifier.
        if (_lookup.TryGetCluster(feature.ClusterId, out var cluster))
        {
            // Distribute the proxy request to this tenant cluster.
            context.ReassignProxyRequest(cluster);
        }

        await _next(context);
    }
}
