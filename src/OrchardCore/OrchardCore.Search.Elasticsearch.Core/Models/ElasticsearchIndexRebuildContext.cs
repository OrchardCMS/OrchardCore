using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexRebuildContext
{
    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexRebuildContext(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
