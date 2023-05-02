using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Clusters;

public static class ClustersStartupExtensions
{
    /// <summary>
    /// Registers tenant clusters components.
    /// </summary>
    public static IReverseProxyBuilder AddClusters(this IReverseProxyBuilder builder)
    {
        return builder.AddConfigFilter<ClustersProxyConfigFilter>();
    }

    /// <summary>
    /// Distributes requests across tenant clusters, should be placed first in the proxy pipeline.
    /// </summary>
    public static IReverseProxyApplicationBuilder UseClusters(this IReverseProxyApplicationBuilder builder)
    {
        builder.UseMiddleware<ClustersProxyMiddleware>();
        return builder;
    }
}
