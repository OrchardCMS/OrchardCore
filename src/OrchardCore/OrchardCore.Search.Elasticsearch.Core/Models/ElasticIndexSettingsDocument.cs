using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticIndexSettingsDocument : Document
{
    public Dictionary<string, ElasticIndexSettings> ElasticIndexSettings { get; set; } = [];
}
