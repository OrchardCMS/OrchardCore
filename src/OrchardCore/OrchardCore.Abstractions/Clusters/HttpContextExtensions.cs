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
        if (!options.Enabled || context.FromClustersProxy())
        {
            return false;
        }

        var host = GetRequestHost(context);

        return options.Hosts.Contains(host);
    }

    /// <summary>
    /// Checks if the current request comes from a clusters proxy.
    /// </summary>
    public static bool FromClustersProxy(this HttpContext context) =>
        context.Request.Headers.TryGetValue("From-Clusters-Proxy", out _);

    public static string GetRequestHost(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Original-Host", out var hosts))
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
