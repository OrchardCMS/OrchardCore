using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

public sealed class RouteRateLimit
{
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

    public string RouteName { get; }

    public IReadOnlyList<string> HttpMethods { get; }

    public Func<HttpContext, RateLimitPartition<string>> Partitioner { get; }
}
