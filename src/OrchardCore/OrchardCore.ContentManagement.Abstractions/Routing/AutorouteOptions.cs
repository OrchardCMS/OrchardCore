using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContentManagement.Routing
{
    public class AutorouteOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string ContentItemIdKey { get; set; } = "";

        // The contained content item key is only used for route generation and should be removed after generation.
        public string ContainedContentItemIdKey { get; set; } = "";
        public string JsonPathKey { get; set; } = "";
    }
}
