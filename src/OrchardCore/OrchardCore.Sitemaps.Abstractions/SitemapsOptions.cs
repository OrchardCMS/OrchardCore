using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Sitemaps
{
    public class SitemapsOptions
    {
        public RouteValueDictionary GlobalRouteValues { get; set; } = new RouteValueDictionary();
        public string SitemapIdKey { get; set; } = "";
    }
}
