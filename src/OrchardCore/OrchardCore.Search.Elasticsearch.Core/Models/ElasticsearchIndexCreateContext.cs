using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexCreateContext
{
    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexCreateContext(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
