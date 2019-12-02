using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContentManagement.Routing
{
    public class AutorouteOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string ContentItemIdKey { get; set; } = "";
    }
}
