using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Sitemaps;

public class SitemapsOptions
{
    public RouteValueDictionary GlobalRouteValues { get; set; } = [];
    public string SitemapIdKey { get; set; } = "";
}
