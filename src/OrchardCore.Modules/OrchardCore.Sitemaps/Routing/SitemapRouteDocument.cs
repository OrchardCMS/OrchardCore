using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Sitemaps.Routing;

public class SitemapRouteDocument : Document
{
    private readonly Dictionary<string, string> _sitemapIds = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> SitemapIds
    {
        get => _sitemapIds;
        set => _sitemapIds.SetItems(value);
    }

    public Dictionary<string, string> SitemapPaths { get; set; } = [];
}
