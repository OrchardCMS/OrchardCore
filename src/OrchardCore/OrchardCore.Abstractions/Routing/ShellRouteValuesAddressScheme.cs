using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    /// <summary>
    /// Allows a tenant to add its own 'RouteValuesAddress' schemes used for link generation.
    /// </summary>
    public sealed class ShellRouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IShellRouteValuesAddressScheme> _schemes;

        private bool _defaultSchemeInitialized;
        private IEndpointAddressScheme<RouteValuesAddress> _defaultScheme;

        public ShellRouteValuesAddressScheme(IHttpContextAccessor httpContextAccessor, IEnumerable<IShellRouteValuesAddressScheme> schemes)
        {
            _httpContextAccessor = httpContextAccessor;
            _schemes = schemes;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            // Run custom tenant schemes.
            foreach (var scheme in _schemes)
            {
                var endpoints = scheme.FindEndpoints(address);

                if (endpoints.Any())
                {
                    return endpoints;
                }
            }

            if (!_defaultSchemeInitialized)
            {
                lock (this)
                {
                    // Try once to get and cache the default scheme but not me.
                    _defaultScheme = _httpContextAccessor.HttpContext?.RequestServices
                        .GetServices<IEndpointAddressScheme<RouteValuesAddress>>()
                        .Where(scheme => scheme.GetType() != GetType())
                        .LastOrDefault();

                    _defaultSchemeInitialized = true;
                }
            }

            // Fallback to the default 'RouteValuesAddress' scheme.
            return _defaultScheme?.FindEndpoints(address) ?? Enumerable.Empty<Endpoint>();
        }
    }
}
