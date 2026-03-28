using System.Text.Json.Nodes;

namespace OrchardCore.Search.OpenSearch;

public class OpenSearchOptions
{
    public string IndexPrefix { get; set; }

    public Dictionary<string, JsonObject> Analyzers { get; } = [];

    public Dictionary<string, JsonObject> TokenFilters { get; } = [];
}
