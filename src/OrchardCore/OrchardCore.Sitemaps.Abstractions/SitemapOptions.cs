using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Sitemaps
{
    public class SitemapOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string SitemapIdKey { get; set; } = "";
    }
}
