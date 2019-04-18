using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;
using OrchardCore.Routing;

namespace OrchardCore.Autoroute.Routing
{
    internal sealed class AutoRouteValuesAddressScheme : IShellRouteValuesAddressScheme
    {
        private readonly IAutorouteEntries _entries;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutoRouteValuesAddressScheme(IAutorouteEntries entries, IHttpContextAccessor httpContextAccessor)
        {
            _entries = entries;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address.AmbientValues == null || address.ExplicitValues == null)
            {
                return Enumerable.Empty<Endpoint>();
            }

            string contentItemId = address.ExplicitValues["contentItemId"]?.ToString();

            if (string.IsNullOrEmpty(contentItemId) || !_entries.TryGetPath(contentItemId, out var path))
            {
                return Enumerable.Empty<Endpoint>();
            }

            var autoRoute = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AutoRoute>();
            var routeValues = autoRoute.GetValuesAsync(contentItemId).GetAwaiter().GetResult();

            if (Match(address.ExplicitValues, routeValues))
            {
                var endpoint = new RouteEndpoint
                (
                    c => null,
                    RoutePatternFactory.Parse(path, address.ExplicitValues, null),
                    0,
                    null,
                    null
                );

                return new[] { endpoint };
            }

            return Enumerable.Empty<Endpoint>();
        }

        private bool Match(RouteValueDictionary explicitValues, RouteValueDictionary routeValues)
        {
            return
                String.Equals(explicitValues["area"]?.ToString(), routeValues["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(explicitValues["controller"]?.ToString(), routeValues["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                String.Equals(explicitValues["action"]?.ToString(), routeValues["action"]?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
