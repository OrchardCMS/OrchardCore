using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;

namespace OrchardCore.Sitemaps.Routing;

internal sealed class SitemapValuesAddressScheme : IShellRouteValuesAddressScheme
{
    private readonly SitemapEntries _entries;
    private readonly SitemapsOptions _options;
    private readonly ConcurrentDictionary<RouteEndpointKey, RouteEndpoint[]> _endpointCache = new();

    public SitemapValuesAddressScheme(SitemapEntries entries, IOptions<SitemapsOptions> options)
    {
        _entries = entries;
        _options = options.Value;
    }

    public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
    {
        if (address.AmbientValues == null || address.ExplicitValues == null)
        {
            return [];
        }

        var sitemapId = address.ExplicitValues[_options.SitemapIdKey]?.ToString();

        if (string.IsNullOrEmpty(sitemapId))
        {
            return [];
        }

        var task = _entries.TryGetPathBySitemapIdAsync(sitemapId);

        bool found;
        string path;

        if (!task.IsCompletedSuccessfully)
        {
            task.GetAwaiter().GetResult(); // Wait for the task to complete if it hasn't already.
        }

        (found, path) = task.Result;

        if (!found)
        {
            return [];
        }

        if (Match(address.ExplicitValues))
        {
            var routeValues = new RouteValueDictionary(address.ExplicitValues);

            if (address.ExplicitValues.Count > _options.GlobalRouteValues.Count + 1)
            {
                foreach (var entry in address.ExplicitValues)
                {
                    if (string.Equals(entry.Key, _options.SitemapIdKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!_options.GlobalRouteValues.ContainsKey(entry.Key))
                    {
                        routeValues.Remove(entry.Key);
                    }
                }
            }

            // RouteEndpoint instances are cached as the internal ASP.NET DefaultLinkGenerator caches them by reference (as a key)
            // c.f. https://github.com/OrchardCMS/OrchardCore/issues/17984

            return _endpointCache.GetOrAdd
            (
                new RouteEndpointKey(path, routeValues),
                static key => {
                    var endpoint = new RouteEndpoint
                    (
                        c => null,
                        RoutePatternFactory.Parse(key.Path, key.RouteValues, null),
                        0,
                        null,
                        null
                    );

                    return [endpoint];
                }
            );
        }

        return [];
    }

    private bool Match(RouteValueDictionary explicitValues)
    {
        foreach (var entry in _options.GlobalRouteValues)
        {
            if (!string.Equals(explicitValues[entry.Key]?.ToString(), entry.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
