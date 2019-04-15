using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace OrchardCore.HomeRoute.Routing
{
    internal sealed class HomeRouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
        private readonly HomeRoute _homeRoute;

        public HomeRouteValuesAddressScheme(HomeRoute homeRoute)
        {
            _homeRoute = homeRoute;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address.AmbientValues == null || address.ExplicitValues == null)
            {
                return Enumerable.Empty<Endpoint>();
            }

            string contentItemId = address.ExplicitValues["contentItemId"]?.ToString();

            if (string.IsNullOrEmpty(contentItemId))
            {
                return Enumerable.Empty<Endpoint>();
            }

            var explicitValues = address.ExplicitValues;

            var routeValues = _homeRoute.GetValuesAsync().GetAwaiter().GetResult();

            if (string.Equals(explicitValues["area"]?.ToString(), routeValues?["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(explicitValues["controller"]?.ToString(), routeValues?["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(explicitValues["action"]?.ToString(), routeValues?["action"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(explicitValues["contentItemId"]?.ToString(), routeValues?["contentItemId"]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var endpoint = new RouteEndpoint
                (
                    c => null,
                    RoutePatternFactory.Parse(String.Empty, explicitValues, null),
                    0,
                    null,
                    null
                );

                return new[] { endpoint };
            }

            return Enumerable.Empty<Endpoint>();
        }
    }
}
