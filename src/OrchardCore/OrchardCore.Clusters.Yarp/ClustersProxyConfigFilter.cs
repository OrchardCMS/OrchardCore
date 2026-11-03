using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace OrchardCore.Clusters;

/// <summary>
/// Proxy config filter that customizes the tenant clusters 'RouteTemplate'.
/// </summary>
public sealed class ClustersProxyConfigFilter : IProxyConfigFilter
{
    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel) => new(origCluster);

    /// <summary>
    /// Customizes the tenant clusters 'RouteTemplate'.
    /// </summary>
    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
    {
        if (route.RouteId != ClustersOptions.RouteTemplate)
        {
            return new ValueTask<RouteConfig>(route);
        }

        // Backends resolve the tenant from the request, so they need the host requested by the client.
        return new ValueTask<RouteConfig>(route.WithTransformUseOriginalHostHeader());
    }
}
