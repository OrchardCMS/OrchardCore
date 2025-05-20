using System.Text.Json.Nodes;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexSettingsExportingContext
{
    public JsonNode Data { get; }

    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexSettingsExportingContext(ElasticIndexSettings settings, JsonNode data)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        Data = data ?? new JsonObject();
    }
}
