using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;

namespace OrchardCore.RateLimits;

/// <summary>
/// Configures the ASP.NET Core rate-limiter pipeline for Orchard Core tenant policies and built-in route limits.
/// </summary>
public sealed class RateLimiterOptionsConfigurations : IConfigureOptions<RateLimiterOptions>
{
    private readonly RateLimitsOptions _rateLimitsOptions;
    private readonly IOptionsMonitor<RateLimitPoliciesOptions> _policiesOptions;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterOptionsConfigurations"/> class.
    /// </summary>
    /// <param name="rateLimitsOptions">The built-in route rate-limit options.</param>
    /// <param name="policiesOptions">The options that provide the current enabled policies.</param>
    /// <param name="serviceProvider">The service provider used to resolve limiter sources.</param>
    public RateLimiterOptionsConfigurations(
        IOptions<RateLimitsOptions> rateLimitsOptions,
        IOptionsMonitor<RateLimitPoliciesOptions> policiesOptions,
        IServiceProvider serviceProvider)
    {
        _rateLimitsOptions = rateLimitsOptions.Value;
        _policiesOptions = policiesOptions;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Configures the chained tenant limiter used for the current shell.
    /// </summary>
    /// <remarks>
    /// The limiters of the enabled policies are replaced whenever the <see cref="RateLimitPoliciesOptions"/> change,
    /// while the global limiter and the limiters of the built-in route and group limits are kept.
    /// </remarks>
    /// <param name="options">The rate-limiter options to configure.</param>
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

        foreach (var groupRateLimit in _rateLimitsOptions.GroupRateLimits)
        {
            limiters.Add(PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (!EndpointBelongsToGroup(context, groupRateLimit.GroupName))
                {
                    return RateLimitPartition.GetNoLimiter(groupRateLimit.GroupName);
                }

                return groupRateLimit.Partitioner(context);
            }));
        }

        limiters.Add(new PolicyRateLimiter(_policiesOptions, CreatePolicyLimiters));

        options.GlobalLimiter = PartitionedRateLimiter.CreateChained([.. limiters]);
    }

    private List<PartitionedRateLimiter<HttpContext>> CreatePolicyLimiters(IReadOnlyCollection<RateLimitPolicy> policies)
    {
        var limiters = new List<PartitionedRateLimiter<HttpContext>>();

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

        return limiters;
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
            RateLimitPolicyScope.Endpoint => !string.IsNullOrWhiteSpace(policy.Path) && context.Request.Path.StartsWithSegments(policy.Path, StringComparison.OrdinalIgnoreCase),
            RateLimitPolicyScope.Group => !string.IsNullOrWhiteSpace(policy.GroupName) && EndpointBelongsToGroup(context, policy.GroupName),
            _ => false,
        };

    private static bool EndpointBelongsToGroup(HttpContext context, string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return false;
        }

        return GetEndpointGroupNames(context).Contains(groupName, StringComparer.OrdinalIgnoreCase);
    }

    private static string[] GetEndpointGroupNames(HttpContext context)
    {
        return context.GetEndpoint()?.Metadata
            .OfType<IRateLimitGroupMetadata>()
            .SelectMany(static metadata => metadata.GroupNames)
            .Where(static groupName => !string.IsNullOrWhiteSpace(groupName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }
}
