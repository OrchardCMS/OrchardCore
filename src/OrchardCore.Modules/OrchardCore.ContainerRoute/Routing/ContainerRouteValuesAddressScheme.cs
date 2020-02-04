using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;

namespace OrchardCore.ContainerRoute.Routing
{
    internal sealed class ContainerRouteValuesAddressScheme : IShellRouteValuesAddressScheme
    {
        private readonly IContainerRouteEntries _entries;
        private readonly ContainerRouteOptions _options;

        public ContainerRouteValuesAddressScheme(IContainerRouteEntries entries, IOptions<ContainerRouteOptions> options)
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

            // Try to get the contained item first, then the container content item
            string contentItemId = address.ExplicitValues[_options.ContainedContentItemIdKey]?.ToString();
            if (string.IsNullOrEmpty(contentItemId))
            {
                contentItemId = address.ExplicitValues[_options.ContainerContentItemIdKey]?.ToString();
            }

            if (string.IsNullOrEmpty(contentItemId) || !_entries.TryGetContainerRouteEntryByContentItemId(contentItemId, out var containerRouteEntry))
            {
                return Enumerable.Empty<Endpoint>();
            }

            if (Match(address.ExplicitValues))
            {
                // Once we have the contained content item id value we no longer want it in the route values.
                address.ExplicitValues.Remove(_options.ContainedContentItemIdKey);

                var routeValues = new RouteValueDictionary(address.ExplicitValues);

                if (address.ExplicitValues.Count > _options.GlobalRouteValues.Count + 1)
                {
                    foreach (var entry in address.ExplicitValues)
                    {
                        if (String.Equals(entry.Key, _options.ContainerContentItemIdKey, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!_options.GlobalRouteValues.ContainsKey(entry.Key))
                        {
                            routeValues.Remove(entry.Key);
                        }
                    }
                }

                var endpoint = new RouteEndpoint
                (
                    c => null,
                    RoutePatternFactory.Parse(containerRouteEntry.Path, routeValues, null),
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
