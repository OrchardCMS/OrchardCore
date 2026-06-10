using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.RateLimits;

/// <summary>
/// Stores read-only route-based rate limits contributed by features in code.
/// </summary>
public sealed class RateLimitsOptions
{
    private readonly Dictionary<string, RouteRateLimit> _routeRateLimits = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registered built-in route rate limits.
    /// </summary>
    public IEnumerable<RouteRateLimit> RouteRateLimits
        => _routeRateLimits.Values;

    /// <summary>
    /// Registers a route rate limit that applies to all HTTP methods for the specified route name.
    /// </summary>
    /// <param name="routeName">The named route to protect.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public void AddRouteRateLimit(string routeName, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        AddRouteRateLimit(routeName, [], partitioner);
    }

    /// <summary>
    /// Registers a route rate limit that applies to a single HTTP method for the specified route name.
    /// </summary>
    /// <param name="routeName">The named route to protect.</param>
    /// <param name="httpMethod">The HTTP method to match.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public void AddRouteRateLimit(string routeName, string httpMethod, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        AddRouteRateLimit(routeName, [httpMethod], partitioner);
    }

    /// <summary>
    /// Registers a route rate limit that applies to the specified HTTP methods for the named route.
    /// </summary>
    /// <param name="routeName">The named route to protect.</param>
    /// <param name="httpMethods">The HTTP methods to match. An empty set matches all methods.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public void AddRouteRateLimit(string routeName, IEnumerable<string> httpMethods, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        var routeRateLimit = new RouteRateLimit(routeName, httpMethods, partitioner);
        _routeRateLimits[CreateKey(routeRateLimit.RouteName, routeRateLimit.HttpMethods)] = routeRateLimit;
    }

    private static string CreateKey(string routeName, IReadOnlyList<string> httpMethods)
    {
        if (httpMethods.Count == 0)
        {
            return routeName;
        }

        return $"{routeName}:{string.Join('|', httpMethods.Order(StringComparer.OrdinalIgnoreCase))}";
    }
}
