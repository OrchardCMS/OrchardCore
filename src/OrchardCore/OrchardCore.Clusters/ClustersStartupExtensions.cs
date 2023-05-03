using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for configuring tenant clusters proxy components on startup.
/// </summary>
public static class ClustersStartupExtensions
{
    /// <summary>
    /// Registers tenant clusters proxy components.
    /// </summary>
    public static IReverseProxyBuilder AddClusters(this IReverseProxyBuilder builder)
    {
        return builder.AddConfigFilter<ClustersProxyConfigFilter>();
    }

    /// <summary>
    /// Distributes proxy requests across tenant clusters, should be placed first in the proxy pipeline.
    /// </summary>
    public static IReverseProxyApplicationBuilder UseClusters(this IReverseProxyApplicationBuilder builder)
    {
        builder.UseMiddleware<ClustersProxyMiddleware>();
        return builder;
    }
}
