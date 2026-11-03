using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;

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

        // Replaces a value sent by the client, and is read on each request to follow the configuration reloads.
        builder.AddTransforms(context =>
        {
            if (context.Route.RouteId != ClustersOptions.RouteTemplate)
            {
                return;
            }

            var optionsMonitor = context.Services.GetRequiredService<IOptionsMonitor<ClustersOptions>>();

            context.AddRequestTransform(transformContext =>
            {
                var headers = transformContext.ProxyRequest.Headers;

                headers.Remove(RequestHeaderNames.FromClustersProxy);
                headers.TryAddWithoutValidation(RequestHeaderNames.FromClustersProxy, optionsMonitor.CurrentValue.ProxySecret);

                return ValueTask.CompletedTask;
            });
        });

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
