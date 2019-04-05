using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    internal sealed class ShellRouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShellRouteValuesAddressScheme(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            // Retrieve all schemes but not me.
            var schemes = _httpContextAccessor.HttpContext?.RequestServices
                .GetServices<IEndpointAddressScheme<RouteValuesAddress>>()
                .Where(x => x.GetType() != GetType())
                .ToArray();

            if (schemes == null || schemes.Length == 0)
            {
                return Enumerable.Empty<Endpoint>();
            }

            // Run tenant level schemes.
            for (var i = 1; i < schemes.Length; i++)
            {
                var endpoints = schemes[i].FindEndpoints(address);

                if (endpoints.Any())
                {
                    return endpoints;
                }
            }

            // Fallback to the global scheme.
            return schemes[0].FindEndpoints(address);
        }
    }
}