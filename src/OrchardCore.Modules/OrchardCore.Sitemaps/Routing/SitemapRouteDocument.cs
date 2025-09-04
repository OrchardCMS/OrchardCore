using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Routing;

public class SitemapRouteDocument : Document
{
    public Dictionary<string, string> SitemapIds { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> SitemapPaths { get; set; } = [];
}
