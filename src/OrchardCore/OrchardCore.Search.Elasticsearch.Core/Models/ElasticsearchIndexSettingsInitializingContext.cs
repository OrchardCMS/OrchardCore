using System.Text.Json.Nodes;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexSettingsInitializingContext
{
    public ElasticIndexSettings Settings { get; }

    public JsonNode Data { get; }

    public ElasticsearchIndexSettingsInitializingContext(ElasticIndexSettings settings, JsonNode data)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        Data = data ?? new JsonObject();
    }
}
