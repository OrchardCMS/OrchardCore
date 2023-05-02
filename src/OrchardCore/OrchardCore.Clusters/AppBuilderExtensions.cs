using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Clusters;

public static class AppBuilderExtensions
{
    /// <summary>
    /// Distributes requests across tenant clusters. This should be placed first in the proxy pipeline.
    /// </summary>
    public static IReverseProxyApplicationBuilder UseClusters(this IReverseProxyApplicationBuilder builder)
    {
        builder.UseMiddleware<ClustersMiddleware>();
        return builder;
    }
}
