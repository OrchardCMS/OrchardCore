using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for configuring the tenant clusters proxy.
/// </summary>
public static class ClustersBuilderExtensions
{
    /// <summary>
    /// Registers tenant clusters proxy components.
    /// </summary>
    public static IReverseProxyBuilder AddTenantClusters(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IConfigureOptions<ClustersOptions>, ClustersOptionsSetup>();
        return builder.AddConfigFilter<ClustersProxyConfigFilter>();
    }

    /// <summary>
    /// Distributes requests across tenant clusters, should be placed first in the proxy pipeline.
    /// </summary>
    public static IReverseProxyApplicationBuilder UseTenantClusters(this IReverseProxyApplicationBuilder builder)
    {
        builder.UseMiddleware<ClustersProxyMiddleware>();
        return builder;
    }
}
