using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using OrchardCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing;

internal sealed class HomeRouteValuesAddressScheme : IShellRouteValuesAddressScheme
{
    private readonly ISiteService _siteService;

    private RouteEndpoint[] _cachedEndpoint;
    private RouteEndpointKey _cachedEndpointKey;

    public HomeRouteValuesAddressScheme(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
    {
        if (address.AmbientValues == null || address.ExplicitValues == null)
        {
            return [];
        }

        var homeRoute = _siteService.GetSiteSettings().HomeRoute;

        if (Match(homeRoute, address.ExplicitValues))
        {
            var routeValues = new RouteValueDictionary(address.ExplicitValues);

            if (address.ExplicitValues.Count > homeRoute.Count)
            {
                foreach (var entry in address.ExplicitValues)
                {
                    if (!homeRoute.ContainsKey(entry.Key))
                    {
                        routeValues.Remove(entry.Key);
                    }
                }
            }

            // RouteEndpoint instances are cached as the internal ASP.NET DefaultLinkGenerator caches them by reference (as a key)
            // c.f. https://github.com/OrchardCMS/OrchardCore/issues/17984

            var endpointKey = new RouteEndpointKey(string.Empty, routeValues);

            var cachedEndpointKey = _cachedEndpointKey;

            if (!cachedEndpointKey.Equals(endpointKey))
            {
                _cachedEndpoint =
                [
                    new RouteEndpoint
                    (
                        c => null,
                        RoutePatternFactory.Parse(string.Empty, routeValues, null),
                        0,
                        null,
                        null
                    ),
                ];

                _cachedEndpointKey = endpointKey;
            }

            return _cachedEndpoint;
        }

        return [];
    }

    private static bool Match(RouteValueDictionary routeValues, RouteValueDictionary explicitValues)
    {
        if (routeValues.Count == 0)
        {
            return false;
        }

        foreach (var entry in routeValues)
        {
            if (!string.Equals(explicitValues[entry.Key]?.ToString(), entry.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
