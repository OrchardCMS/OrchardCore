using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy;

namespace OrchardCore.Clusters;

/// <summary>
/// Distributes tenant requests across shell clusters.
/// </summary>
public class ClustersMiddleware
{
    const int _slotsCount = 16384;
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _proxyStateLookup;
    private readonly ClustersOptions _options;

    public ClustersMiddleware(
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
        // If this instance is not used as a reverse proxy...
        if (!context.UseAsClustersProxy(_options))
        {
            // Bypass the clusters middleware.
            await _next(context);
            return;
        }

        var tenantId = context.GetClusterFeature()?.TenantId;
        if (tenantId is not null)
        {
            var reverseProxyFeature = context.GetReverseProxyFeature();
            var slotHash = Crc16XModem.Compute(tenantId) % _slotsCount;
            foreach (var clusterOption in _options.Clusters)
            {
                if (clusterOption.SlotMin <= slotHash && clusterOption.SlotMax >= slotHash)
                {
                    if (clusterOption.ClusterId != reverseProxyFeature.Cluster.Config.ClusterId &&
                        _proxyStateLookup.TryGetCluster(clusterOption.ClusterId, out var cluster))
                    {
                        context.ReassignProxyRequest(cluster);
                    }

                    break;
                }
            }
        }

        await _next(context);
    }
}
