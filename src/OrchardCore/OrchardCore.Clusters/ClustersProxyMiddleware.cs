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
        // Check if this instance is not used as a reverse proxy.
        if (!context.AsClustersProxy(_options))
        {
            // Bypass the clusters proxy middleware.
            await _next(context);
            return;
        }

        // Get the tenant identifier from the cluster feature.
        var tenantId = context.GetClusterFeature()?.TenantId;
        if (tenantId is not null)
        {
            // Compute the hash of the current tenant from its identifier.
            var tenantHash = Crc16XModem.Compute(tenantId) % _slotsCount;
            foreach (var clusterOptions in _options.Clusters)
            {
                // Check if the slot of the current tenant belongs to this cluster.
                if (clusterOptions.SlotMin > tenantHash || clusterOptions.SlotMax < tenantHash)
                {
                    continue;
                }

                // Try to get the configured cluster with the same identifier.
                if (_lookup.TryGetCluster(clusterOptions.ClusterId, out var cluster))
                {
                    // Distribute the proxy request to this tenant cluster.
                    context.ReassignProxyRequest(cluster);
                }

                break;
            }
        }

        await _next(context);
    }
}
