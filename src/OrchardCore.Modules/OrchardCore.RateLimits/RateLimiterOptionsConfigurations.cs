using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits;

public sealed class RateLimiterOptionsConfigurations : IConfigureOptions<RateLimiterOptions>
{
    private readonly RateLimitsOptions _rateLimitsOptions;
    private readonly IRateLimitPolicyStore _policyStore;
    private readonly IServiceProvider _serviceProvider;

    public RateLimiterOptionsConfigurations(
        IOptions<RateLimitsOptions> rateLimitsOptions,
        IRateLimitPolicyStore policyStore,
        IServiceProvider serviceProvider)
    {
        _rateLimitsOptions = rateLimitsOptions.Value;
        _policyStore = policyStore;
        _serviceProvider = serviceProvider;
    }

    public void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        var limiters = new List<PartitionedRateLimiter<HttpContext>>
        {
            PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var routeRateLimit = FindRouteRateLimit(context);
                if (routeRateLimit is null)
                {
                    return RateLimitPartition.GetNoLimiter(GetEndpointRouteName(context) ?? "default");
                }

                return routeRateLimit.Partitioner(context);
            }),
        };

        var policies = _policyStore.GetPublishedPoliciesAsync().GetAwaiter().GetResult();

        foreach (var policy in policies)
        {
            foreach (var limiter in policy.Limiters)
            {
                var source = _serviceProvider.GetKeyedService<IRateLimiterSource>(limiter.Source);
                if (source is null)
                {
                    continue;
                }

                limiters.Add(PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    if (!Matches(policy, context))
                    {
                        return RateLimitPartition.GetNoLimiter(policy.Name);
                    }

                    return source.CreatePartition(policy.Name, context, limiter);
                }));
            }
        }

        options.GlobalLimiter = limiters.Count switch
        {
            0 => PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetNoLimiter("default")),
            1 => limiters[0],
            _ => PartitionedRateLimiter.CreateChained([.. limiters]),
        };
    }

    private RouteRateLimit FindRouteRateLimit(HttpContext context)
    {
        var routeName = GetEndpointRouteName(context);
        if (string.IsNullOrEmpty(routeName))
        {
            return null;
        }

        foreach (var routeRateLimit in _rateLimitsOptions.RouteRateLimits)
        {
            if (!string.Equals(routeRateLimit.RouteName, routeName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (routeRateLimit.HttpMethods.Count > 0 &&
                !routeRateLimit.HttpMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            return routeRateLimit;
        }

        return null;
    }

    private static string GetEndpointRouteName(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        return endpoint?.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName
            ?? endpoint?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
    }

    private static bool Matches(RateLimitPolicy policy, HttpContext context)
        => policy.Scope switch
        {
            RateLimitPolicyScope.Global => true,
            RateLimitPolicyScope.Route => string.Equals(policy.RouteName, GetEndpointRouteName(context), StringComparison.OrdinalIgnoreCase),
            RateLimitPolicyScope.Endpoint => !string.IsNullOrWhiteSpace(policy.Path) && context.Request.Path.StartsWithSegments(policy.Path, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
}
