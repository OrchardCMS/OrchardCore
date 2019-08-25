using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;
using OrchardCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    internal sealed class HomeRouteValuesAddressScheme : IShellRouteValuesAddressScheme
    {
        private readonly ISiteService _siteService;
        private RouteValueDictionary _routeValues;
        private IChangeToken _changeToken;

        public HomeRouteValuesAddressScheme(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address.AmbientValues == null || address.ExplicitValues == null)
            {
                return Enumerable.Empty<Endpoint>();
            }

            if (Match(address.ExplicitValues))
            {
                var endpoint = new RouteEndpoint
                (
                    c => null,
                    RoutePatternFactory.Parse(String.Empty, address.ExplicitValues, null),
                    0,
                    null,
                    null
                );

                return new[] { endpoint };
            }

            return Enumerable.Empty<Endpoint>();
        }

        private bool Match(RouteValueDictionary explicitValues)
        {
            if (_changeToken?.HasChanged ?? true)
            {
                var changeToken = _siteService.ChangeToken;
                _routeValues = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult().HomeRoute;
                _changeToken = changeToken;
            }

            foreach (var entry in _routeValues)
            {
                if (!String.Equals(explicitValues[entry.Key]?.ToString(), entry.Value?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
