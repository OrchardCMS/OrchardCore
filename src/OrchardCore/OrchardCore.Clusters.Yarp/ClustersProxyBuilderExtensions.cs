using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for configuring the tenant clusters proxy.
/// </summary>
public static class ClustersProxyBuilderExtensions
{
    /// <summary>
    /// Registers tenant clusters proxy components.
    /// </summary>
    public static IReverseProxyBuilder AddTenantClusters(this IReverseProxyBuilder builder)
    {
        builder.Services.AddSingleton<IConfigureOptions<ClustersOptions>, ClustersOptionsSetup>();

        // An invalid configuration should prevent the host from starting, instead of failing on each request.
        builder.Services.AddOptions<ClustersOptions>().ValidateOnStart();

        builder.Services.AddSingleton<IOptionsChangeTokenSource<ClustersOptions>>(serviceProvider =>
            new ConfigurationChangeTokenSource<ClustersOptions>(
                Options.DefaultName,
                serviceProvider.GetRequiredService<IConfiguration>().GetSection(ClustersOptions.ConfigurationSection)));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ClustersProxyMatcherPolicy>());

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
