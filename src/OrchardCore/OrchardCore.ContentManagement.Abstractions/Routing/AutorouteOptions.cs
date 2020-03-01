using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContentManagement.Routing
{
    public class AutorouteOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string ContentItemIdKey { get; set; } = "";
        public string ContainedContentItemIdKey { get; set; } = "";
        public string JsonPathKey { get; set; } = "";
    }
}
