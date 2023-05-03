using System.Linq;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

public static class HttpContextExtensions
{
    /// <summary>
    /// Checks if this instance runs as a clusters proxy.
    /// </summary>
    public static bool AsClustersProxy(this HttpContext context, ClustersOptions options)
    {
        // Check if enabled and prevents request recursions.
        if (!options.Enabled || context.FromClustersProxy())
        {
            return false;
        }

        var host = GetRequestHost(context);

        // Check if the request host matches one of the clusters proxy hosts.
        return options.Hosts.Contains(host);
    }

    /// <summary>
    /// Checks if the current request comes from a clusters proxy.
    /// </summary>
    public static bool FromClustersProxy(this HttpContext context) =>
        context.Request.Headers.TryGetValue(RequestHeaderNames.FromClustersProxy, out _);


    /// <summary>
    /// Returns the original host header if it exists otherwise the request host.
    /// </summary>
    public static string GetRequestHost(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(RequestHeaderNames.XOriginalHost, out var hosts))
        {
            return context.Request.Host.ToString();
        }

        return hosts.FirstOrDefault();
    }

    /// <summary>
    /// Gets the current <see cref="ClusterFeature"/> holding the current <see cref="ShellSettings.TenantId"/>.
    /// </summary>
    public static ClusterFeature GetClusterFeature(this HttpContext context) =>
        context.Features.Get<ClusterFeature>();
}
