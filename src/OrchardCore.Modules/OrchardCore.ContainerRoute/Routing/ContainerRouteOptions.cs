using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContainerRoute.Routing
{
    public class ContainerRouteOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string ContainerContentItemIdKey { get; set; } = "";
        public string JsonPathKey { get; set; } = "";
        public string ContainedContentItemIdKey { get; set; } = "";
    }
}
