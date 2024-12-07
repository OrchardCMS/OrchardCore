using System.Text.Json.Nodes;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchOptions
{
    public string IndexPrefix { get; set; }

    public Dictionary<string, JsonObject> Analyzers { get; } = [];

    public Dictionary<string, JsonObject> TokenFilters { get; } = [];
}
