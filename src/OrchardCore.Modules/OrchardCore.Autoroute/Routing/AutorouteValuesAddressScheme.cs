using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Routing;

namespace OrchardCore.Autoroute.Routing
{
    internal sealed class AutoRouteValuesAddressScheme : IShellRouteValuesAddressScheme
    {
        private readonly IAutorouteEntries _entries;
        private readonly AutorouteOptions _options;

        public AutoRouteValuesAddressScheme(IAutorouteEntries entries, IOptions<AutorouteOptions> options)
        {
            _entries = entries;
            _options = options.Value;
        }

        public IEnumerable<Endpoint> FindEndpoints(RouteValuesAddress address)
        {
            if (address.AmbientValues == null || address.ExplicitValues == null)
            {
                return Enumerable.Empty<Endpoint>();
            }

            string contentItemId = address.ExplicitValues[_options.ContentItemIdKey]?.ToString();

            if (string.IsNullOrEmpty(contentItemId) || !_entries.TryGetPath(contentItemId, out var path))
            {
                return Enumerable.Empty<Endpoint>();
            }

            if (Match(address.ExplicitValues))
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

        private bool Match(RouteValueDictionary explicitValues)
        {
            foreach (var entry in _options.GlobalRouteValues)
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
