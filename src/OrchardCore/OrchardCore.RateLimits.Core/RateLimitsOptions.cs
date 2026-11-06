using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace OrchardCore.RateLimits;

/// <summary>
/// Stores read-only route-based rate limits contributed by features in code.
/// </summary>
public sealed class RateLimitsOptions
{
    private readonly Dictionary<string, RouteRateLimit> _routeRateLimits = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<GroupRateLimit> _groupRateLimits = [];
    private readonly List<PathString> _globalPolicyExcludedPaths = [];
    private readonly Dictionary<string, Func<OnRejectedContext, CancellationToken, ValueTask>> _routeRejectionHandlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registered built-in route rate limits.
    /// </summary>
    public IEnumerable<RouteRateLimit> RouteRateLimits
        => _routeRateLimits.Values;

    /// <summary>
    /// Gets the registered built-in group rate limits.
    /// </summary>
    public IEnumerable<GroupRateLimit> GroupRateLimits
        => _groupRateLimits;

    /// <summary>
    /// Gets the request path prefixes that global policies don't count, such as the paths that serve static files.
    /// </summary>
    public IEnumerable<PathString> GlobalPolicyExcludedPaths
        => _globalPolicyExcludedPaths;

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

    /// <summary>
    /// Registers a group rate limit that applies to endpoints assigned to the specified group.
    /// </summary>
    /// <param name="groupName">The endpoint group to protect.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public void AddGroupRateLimit(string groupName, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        _groupRateLimits.Add(new GroupRateLimit(groupName, partitioner));
    }

    /// <summary>
    /// Excludes the requests whose path starts with the specified prefix from global policies, so that serving static files,
    /// such as the images of a page, doesn't consume the request budget of a client. Endpoint and group policies still apply.
    /// </summary>
    /// <param name="pathPrefix">The request path prefix to exclude, such as <c>/media</c>.</param>
    public void ExcludePathFromGlobalPolicies(PathString pathPrefix)
    {
        if (!pathPrefix.HasValue || pathPrefix == "/")
        {
            throw new ArgumentException("The path prefix must not be empty or the root path.", nameof(pathPrefix));
        }

        if (!_globalPolicyExcludedPaths.Contains(pathPrefix))
        {
            _globalPolicyExcludedPaths.Add(pathPrefix);
        }
    }

    /// <summary>
    /// Registers the handler that writes the response of a request to the specified named route when any active limiter
    /// rejects it, such as an endpoint that must return an error in the format its clients expect. The response status code
    /// is set to <c>429 Too Many Requests</c> before the handler is invoked.
    /// </summary>
    /// <param name="routeName">The named route whose rejected requests the handler responds to.</param>
    /// <param name="onRejected">The handler that writes the response of a rejected request.</param>
    public void AddRouteRejectionHandler(string routeName, Func<OnRejectedContext, CancellationToken, ValueTask> onRejected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeName);
        ArgumentNullException.ThrowIfNull(onRejected);

        _routeRejectionHandlers[routeName] = onRejected;
    }

    /// <summary>
    /// Gets the handler that writes the response of a rejected request to the specified named route.
    /// </summary>
    /// <param name="routeName">The named route of the rejected request.</param>
    /// <param name="onRejected">The registered handler, or <see langword="null"/> when the route has none.</param>
    /// <returns><see langword="true"/> when a handler is registered for the route; otherwise, <see langword="false"/>.</returns>
    public bool TryGetRouteRejectionHandler(string routeName, out Func<OnRejectedContext, CancellationToken, ValueTask> onRejected)
    {
        if (string.IsNullOrEmpty(routeName))
        {
            onRejected = null;

            return false;
        }

        return _routeRejectionHandlers.TryGetValue(routeName, out onRejected);
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
