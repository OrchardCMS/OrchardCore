using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace OrchardCore.Clusters;

/// <summary>
/// Proxy config filter that customizes the tenant clusters 'RouteTemplate'.
/// </summary>
public class ClustersProxyConfigFilter : IProxyConfigFilter
{
    private readonly ClustersOptions _options;

    public ClustersProxyConfigFilter(IOptions<ClustersOptions> options) => _options = options.Value;

    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel) => new(origCluster);

    /// <summary>
    /// Customizes the tenant clusters 'RouteTemplate'.
    /// </summary>
    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
    {
        // Check if it is the tenant clusters 'RouteTemplate'.
        if (route.RouteId == ClustersOptions.RouteTemplate)
        {
            // Define the headers that incoming requests should match.
            var headers = new List<RouteHeader>
            {
                new RouteHeader
                {
                    Name = _options.Enabled ? RequestHeaderNames.FromClustersProxy : RequestHeaderNames.FakeClustersHeader,
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
