using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy;

namespace OrchardCore.Clusters;

/// <summary>
/// Distributes proxy requests across tenant clusters.
/// </summary>
public class ClustersProxyMiddleware
{
    const int _slotsCount = 16384;
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _lookup;
    private readonly ClustersOptions _options;

    public ClustersProxyMiddleware(
        RequestDelegate next,
        IProxyStateLookup proxyStateLookup,
        IOptions<ClustersOptions> options)
    {
        _next = next;
        _lookup = proxyStateLookup;
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
                var slotHash = Crc16XModem.Compute(tenantId) % _slotsCount;
                foreach (var clusterOptions in _options.Clusters)
                {
                    // Check if the slot of the current tenant belongs to this cluster.
                    if (clusterOptions.SlotMin > slotHash || clusterOptions.SlotMax < slotHash)
                    {
                        continue;
                    }

                    // Check if a configured cluster with the same identifier exists.
                    if (_lookup.TryGetCluster(clusterOptions.ClusterId, out var cluster))
                    {
                        // Distribute the proxy request to this tenant cluster.
                        context.ReassignProxyRequest(cluster);
                    }

                    break;
                }
            }
        }

        await _next(context);
    }
}
