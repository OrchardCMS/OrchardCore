using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace OrchardCore.Clusters;

/// <summary>
/// Proxy configuration filter that customizes the 'RouteTemplate' used by the clusters proxy.
/// </summary>
public class ClustersProxyConfigFilter : IProxyConfigFilter
{
    private readonly ClustersOptions _options;

    public ClustersProxyConfigFilter(IOptions<ClustersOptions> options) => _options = options.Value;

    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel) => new(origCluster);

    /// <summary>
    /// Customizes the 'RouteTemplate' used by the clusters proxy.
    /// </summary>
    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
    {
        // Check if it is the route template used by the clusters proxy.
        if (route.RouteId == ClustersOptions.RouteTemplate)
        {
            // Define the headers that incoming requests should match.
            var headers = new List<RouteHeader>
            {
                new RouteHeader
                {
                    Name = _options.Enabled ? RequestHeaderNames.FromClustersProxy : RequestHeaderNames.CheckClustersProxy,
                    Mode = _options.Enabled ? HeaderMatchMode.NotExists : HeaderMatchMode.Exists,
                }
            };

            // Preserve the already defined headers.
            if (route.Match.Headers is not null)
            {
                headers.AddRange(route.Match.Headers);
            }

            return new ValueTask<RouteConfig>((route with
            {
                Match = route.Match with
                {
                    // Set the uri hosts that incoming requests should match.
                    Hosts = _options.Hosts,
                    Headers = headers,
                },
            })
                // Defines a header to be sent by the proxy request.
                .WithTransformRequestHeader(RequestHeaderNames.FromClustersProxy, "true"));
        }

        return new ValueTask<RouteConfig>(route);
    }
}
