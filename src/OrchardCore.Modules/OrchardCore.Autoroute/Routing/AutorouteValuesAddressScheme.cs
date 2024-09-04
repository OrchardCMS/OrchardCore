using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Routing;

namespace OrchardCore.Autoroute.Routing;

internal sealed class AutorouteValuesAddressScheme : IShellRouteValuesAddressScheme
{
    private readonly IAutorouteEntries _entries;
    private readonly AutorouteOptions _options;

    public AutorouteValuesAddressScheme(IAutorouteEntries entries, IOptions<AutorouteOptions> options)
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

        // Try to get the contained item first, then the container content item
        var contentItemId = address.ExplicitValues[_options.ContainedContentItemIdKey]?.ToString();
        if (string.IsNullOrEmpty(contentItemId))
        {
            contentItemId = address.ExplicitValues[_options.ContentItemIdKey]?.ToString();
        }

        if (string.IsNullOrEmpty(contentItemId))
        {
            return [];
        }

        (var found, var autorouteEntry) = _entries.TryGetEntryByContentItemIdAsync(contentItemId).GetAwaiter().GetResult();

        if (!found)
        {
            return [];
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
                    if (string.Equals(entry.Key, _options.ContentItemIdKey, StringComparison.OrdinalIgnoreCase))
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
                RoutePatternFactory.Parse(autorouteEntry.Path, routeValues, null),
                0,
                null,
                null
            );

            return new[] { endpoint };
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
