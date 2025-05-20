using OrchardCore.Indexing;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class ElasticsearchIndexMap
{
    public string IndexingKey { get; set; }

    public string ElasticsearchFieldKey { get; set; }

    public DocumentIndexOptions Options { get; set; }

    public Types Type { get; set; }

    public ElasticsearchIndexMap(string azureFieldKey, Types type)
    {
        ArgumentException.ThrowIfNullOrEmpty(azureFieldKey);

        ElasticsearchFieldKey = azureFieldKey;
        Type = type;
    }

    public ElasticsearchIndexMap(string elasticsearchFieldKey, Types type, DocumentIndexOptions options)
        : this(elasticsearchFieldKey, type)
    {
        Options = options;
    }
}
