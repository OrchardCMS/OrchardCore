using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Sitemaps.Routing;

public class SitemapRouteDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string>))]
    public Dictionary<string, string> SitemapIds { get; init; }

    public Dictionary<string, string> SitemapPaths { get; set; } = [];
}
