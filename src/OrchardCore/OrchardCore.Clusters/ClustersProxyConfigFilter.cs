using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace OrchardCore.Clusters;

public class ClustersProxyConfigFilter : IProxyConfigFilter
{
    private readonly ClustersOptions _options;

    public ClustersProxyConfigFilter(IOptions<ClustersOptions> options) => _options = options.Value;

    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig origCluster, CancellationToken cancel) => new(origCluster);

    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
    {
        if (route.RouteId == "RouteTemplate")
        {
            var headers = new List<RouteHeader>
            {
                new RouteHeader
                {
                    Name = _options.Enabled ? "From-Clusters-Proxy" : "Check-Clusters-Proxy",
                    Mode = _options.Enabled ? HeaderMatchMode.NotExists : HeaderMatchMode.Exists,
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
                    Hosts = _options.Hosts,
                    Headers = headers,
                },
            })
                .WithTransformRequestHeader("From-Clusters-Proxy", "true"));
        }

        return new ValueTask<RouteConfig>(route);
    }
}
