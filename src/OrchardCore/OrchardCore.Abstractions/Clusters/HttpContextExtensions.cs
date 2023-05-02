using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Clusters;

public static class HttpContextExtensions
{
    public static bool AsClustersProxy(this HttpContext context, ClustersOptions options)
    {
        if (!options.Enabled || context.IsFromClustersProxy())
        {
            return false;
        }

        var host = GetRequestHost(context);

        return options.Hosts.Contains(host);
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
