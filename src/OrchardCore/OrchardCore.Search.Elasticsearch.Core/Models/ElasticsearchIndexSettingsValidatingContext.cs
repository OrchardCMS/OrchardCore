using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Models;

public class ElasticsearchIndexSettingsValidatingContext
{
    public ValidationResultDetails Result { get; } = new();

    public ElasticIndexSettings Settings { get; }

    public ElasticsearchIndexSettingsValidatingContext(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}
