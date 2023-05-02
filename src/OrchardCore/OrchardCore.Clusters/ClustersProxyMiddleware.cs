using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy;

namespace OrchardCore.Clusters;

/// <summary>
/// Distributes tenant requests across shell clusters.
/// </summary>
public class ClustersProxyMiddleware
{
    const int _slotsCount = 16384;
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _proxyStateLookup;
    private readonly ClustersOptions _options;

    public ClustersProxyMiddleware(
        RequestDelegate next,
        IProxyStateLookup proxyStateLookup,
        IOptions<ClustersOptions> options)
    {
        _next = next;
        _proxyStateLookup = proxyStateLookup;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        // If this instance is used as a reverse proxy.
        if (context.AsClustersProxy(_options))
        {
            var tenantId = context.GetClusterFeature()?.TenantId;
            if (tenantId is not null)
            {
                var reverseProxyFeature = context.GetReverseProxyFeature();
                var slotHash = Crc16XModem.Compute(tenantId) % _slotsCount;
                foreach (var clusterOptions in _options.Clusters)
                {
                    if (clusterOptions.SlotMin <= slotHash &&
                        clusterOptions.SlotMax >= slotHash &&
                        clusterOptions.ClusterId != reverseProxyFeature.Cluster.Config.ClusterId &&
                        _proxyStateLookup.TryGetCluster(clusterOptions.ClusterId, out var cluster))
                    {
                        context.ReassignProxyRequest(cluster);

                        break;
                    }
                }
            }
        }

        await _next(context);
    }
}
