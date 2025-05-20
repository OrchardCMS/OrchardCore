using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class ElasticsearchIndexSettingsSynchronizedContext
{
    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexSettingsSynchronizedContext(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
