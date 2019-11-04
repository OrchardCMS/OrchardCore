using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Menu.Models
{
    public class ContentPickerMenuItemPart : ContentPart
    {
        /// <summary>
        /// The name of the link
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content Picker ItemIds
        /// </summary>
        public string ContentItemIds { get; set; }

        public RouteValueDictionary GetContentPickerItemRouteValue(AutorouteOptions autoRouteOption)
        {
            var routeValues = new RouteValueDictionary(autoRouteOption.GlobalRouteValues);
            routeValues[autoRouteOption.ContentItemIdKey] = ContentItemIds;
            return routeValues;
        }
    }
}
