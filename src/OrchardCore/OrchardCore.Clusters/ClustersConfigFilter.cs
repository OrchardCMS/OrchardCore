using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace OrchardCore.Clusters;

public class ClustersConfigFilter : IProxyConfigFilter
{
    private readonly ClustersOptions _options;

    public ClustersConfigFilter(IOptions<ClustersOptions> options) => _options = options.Value;

    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel) => new(origCluster);

    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
    {
        if (route.RouteId == "RouteTemplate")
        {
            var headers = new List<RouteHeader>
            {
                new RouteHeader
                {
                    Name = "From-Clusters-Proxy",
                    Mode = HeaderMatchMode.NotExists,
                }
            };

            if (route.Match.Headers is not null)
            {
                headers.AddRange(route.Match.Headers);
            }

            return new ValueTask<RouteConfig>((route with
            {
                Match = route.Match with
                {
                    Hosts = _options.ProxyHosts,
                    Headers = headers,
                },
            })
                .WithTransformRequestHeader("From-Clusters-Proxy", "true"));
        }

        return new ValueTask<RouteConfig>(route);
    }
}
