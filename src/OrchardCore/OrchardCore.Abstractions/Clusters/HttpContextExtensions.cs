using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for checking clusters proxy requests.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Checks if this instance runs as a clusters proxy.
    /// </summary>
    public static bool AsClustersProxy(this HttpContext context, ClustersOptions options) =>
        options.Enabled && !context.FromClustersProxy();

    /// <summary>
    /// Checks if the current request comes from a clusters proxy.
    /// </summary>
    public static bool FromClustersProxy(this HttpContext context) =>
        context.Request.Headers.TryGetValue(RequestHeaderNames.FromClustersProxy, out _);

    /// <summary>
    /// Tries to get the <see cref="ClusterFeature"/> holding the selected tenant cluster identifier.
    /// </summary>
    public static bool TryGetClusterFeature(this HttpContext context, out ClusterFeature feature)
    {
        feature = context.Features.Get<ClusterFeature>();
        return feature != null;
    }

    /// <summary>
    /// Returns the original host header if it exists, otherwise the request host.
    /// </summary>
    public static string GetClustersRequestHost(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(RequestHeaderNames.XOriginalHost, out var hosts))
        {
            return context.Request.Host.ToString();
        }

        return hosts.FirstOrDefault();
    }
}
