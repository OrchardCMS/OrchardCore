using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Clusters;

public static class HttpContextExtensions
{
    public static bool UseAsClustersProxy(this HttpContext context, ClustersOptions options)
    {
        if (!options.UseAsProxy || context.IsFromClustersProxy())
        {
            return false;
        }

        var host = GetRequestHost(context);

        return options.ProxyHosts.Contains(host);
    }

    public static bool IsFromClustersProxy(this HttpContext context) =>
        context.Request.Headers.TryGetValue("From-Clusters-Proxy", out _);

    public static string GetRequestHost(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-Original-Host", out var hosts))
        {
            return context.Request.Host.ToString();
        }

        return hosts.FirstOrDefault();
    }

    public static ClusterFeature GetClusterFeature(this HttpContext context) =>
        context.Features.Get<ClusterFeature>();
}
