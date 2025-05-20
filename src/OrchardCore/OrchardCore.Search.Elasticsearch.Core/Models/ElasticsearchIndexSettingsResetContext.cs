using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexSettingsResetContext
{
    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexSettingsResetContext(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
