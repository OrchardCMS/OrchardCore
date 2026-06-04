using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

public sealed class RateLimitsOptions
{
    private readonly Dictionary<string, RouteRateLimit> _routeRateLimits = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<RouteRateLimit> RouteRateLimits => _routeRateLimits.Values;

    public void AddRouteRateLimit(string routeName, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        AddRouteRateLimit(routeName, [], partitioner);
    }

    public void AddRouteRateLimit(string routeName, string httpMethod, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        AddRouteRateLimit(routeName, [httpMethod], partitioner);
    }

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
