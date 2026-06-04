using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace OrchardCore.RateLimits;

public sealed class RateLimiterOptionsConfigurations : IConfigureOptions<RateLimiterOptions>
{
    private readonly GlobalRateLimitOptions _globalRateLimitOptions;
    private readonly RateLimitsOptions _rateLimitsOptions;

    public RateLimiterOptionsConfigurations(
        IOptions<GlobalRateLimitOptions> globalRateLimitOptions,
        IOptions<RateLimitsOptions> rateLimitsOptions)
    {
        _globalRateLimitOptions = globalRateLimitOptions.Value;
        _rateLimitsOptions = rateLimitsOptions.Value;
    }

    public void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        var routeLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var routeRateLimit = FindRouteRateLimit(context);
            if (routeRateLimit is null)
            {
                return RateLimitPartition.GetNoLimiter(GetEndpointRouteName(context) ?? "default");
            }

            return routeRateLimit.Partitioner(context);
        });


        var globalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitPartitionHelpers.GetRemoteIpAddress(context),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = _globalRateLimitOptions.PermitLimit,
                    Window = _globalRateLimitOptions.Window,
                    QueueLimit = _globalRateLimitOptions.QueueLimit,
                    AutoReplenishment = true,
                }));

        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(routeLimiter, globalLimiter);
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
}
