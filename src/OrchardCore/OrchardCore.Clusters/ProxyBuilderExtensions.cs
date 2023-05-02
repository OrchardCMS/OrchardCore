using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Clusters;

public static class ProxyBuilderExtensions
{
    /// <summary>
    /// Registers tenant clusters services.
    /// </summary>
    public static IReverseProxyBuilder AddClusters(this IReverseProxyBuilder builder)
    {
        return builder.AddConfigFilter<ClustersConfigFilter>();
    }
}
