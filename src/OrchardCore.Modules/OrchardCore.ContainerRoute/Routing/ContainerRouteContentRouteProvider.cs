using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.ContainerRoute.Routing
{
    public class ContainerRouteContentRouteProvider : IContentRouteProvider
    {
        private readonly IContainerRouteEntries _containerRouteEntries;
        private readonly ContainerRouteOptions _options;

        public ContainerRouteContentRouteProvider(
            IContainerRouteEntries containerRouteEntries,
            IOptions<ContainerRouteOptions> options)
        {
            _containerRouteEntries = containerRouteEntries;
            _options = options.Value;
        }

        public bool TryGetContentRouteValues(HttpContext httpContext, out RouteValueDictionary routeValues)
        {
            if (_containerRouteEntries.TryGetContainerRouteEntryByPath(httpContext.Request.Path.Value, out var containerRouteEntry))
            {
                routeValues = new RouteValueDictionary(_options.GlobalRouteValues)
                {
                    [_options.ContainerContentItemIdKey] = containerRouteEntry.ContainerContentItemId,
                    [_options.JsonPathKey] = containerRouteEntry.JsonPath
                };

                return true;
            }

            routeValues = null;

            return false;
        }

        public bool TryGetContentItemId(string path, out string contentItemId)
        {
            if (_containerRouteEntries.TryGetContainerRouteEntryByPath(path, out var containerRouteEntry))
            {
                contentItemId = containerRouteEntry.ContainerContentItemId;

                return true;
            }

            contentItemId = null;

            return false;
        }
    }
}
