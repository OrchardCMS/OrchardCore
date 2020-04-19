using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Menu.Routing
{
    public class ContentPickerItemRoute
    {
        private readonly AutorouteOptions _autoRouteOption;
        public ContentPickerItemRoute(IOptions<AutorouteOptions> autoRouteOption)
        {
            _autoRouteOption = autoRouteOption.Value;
        }

        public RouteValueDictionary GetContentPickerItemRouteValue(string contentItemId)
        {
            var routeValues = new RouteValueDictionary(_autoRouteOption.GlobalRouteValues);
            routeValues[_autoRouteOption.ContentItemIdKey] = contentItemId;
            return routeValues;
        }
    }
}
