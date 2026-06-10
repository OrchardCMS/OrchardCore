using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

/// <summary>
/// Represents a read-only route-based limiter registered in code.
/// </summary>
public sealed class RouteRateLimit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RouteRateLimit"/> class.
    /// </summary>
    /// <param name="routeName">The route name to protect.</param>
    /// <param name="httpMethods">The HTTP methods to match. An empty set matches all methods.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public RouteRateLimit(string routeName, IEnumerable<string> httpMethods, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeName);
        ArgumentNullException.ThrowIfNull(partitioner);

        RouteName = routeName;
        HttpMethods = httpMethods?
            .Where(static method => !string.IsNullOrWhiteSpace(method))
            .Select(static method => method.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
        Partitioner = partitioner;
    }

    /// <summary>
    /// Gets the route name that this limiter matches.
    /// </summary>
    public string RouteName { get; }

    /// <summary>
    /// Gets the HTTP methods that this limiter matches. An empty collection matches all methods.
    /// </summary>
    public IReadOnlyList<string> HttpMethods { get; }

    /// <summary>
    /// Gets the partition factory used to create the limiter for a matching request.
    /// </summary>
    public Func<HttpContext, RateLimitPartition<string>> Partitioner { get; }
}
