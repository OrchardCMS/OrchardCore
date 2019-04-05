using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;

namespace OrchardCore.Autoroute.Routing
{
    internal sealed class AutorouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
        private readonly IAutorouteEntries _entries;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutorouteValuesAddressScheme(IAutorouteEntries entries, IHttpContextAccessor httpContextAccessor)
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

            if (string.IsNullOrEmpty(contentItemId))
            {
                return Enumerable.Empty<Endpoint>();
            }

            var explicitValues = address.ExplicitValues;

            var autorouteRoute = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AutorouteRoute>();
            var routeValues = autorouteRoute.GetValuesAsync(contentItemId).GetAwaiter().GetResult();

            if (string.Equals(explicitValues["area"]?.ToString(), routeValues?["area"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(explicitValues["controller"]?.ToString(), routeValues?["controller"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(explicitValues["action"]?.ToString(), routeValues?["action"]?.ToString(), StringComparison.OrdinalIgnoreCase) &&
                _entries.TryGetPath(explicitValues["contentItemId"].ToString(), out var path))
            {
                var endpoint = new RouteEndpoint
                (
                    c => null,
                    RoutePatternFactory.Parse(path, explicitValues, null),
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
