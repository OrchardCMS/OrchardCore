using OrchardCore.Search.Elasticsearch.Core.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class SearchIndexDefinition
{
    public SearchIndexDefinition(
        ElasticsearchIndexMap indexMap,
        DocumentIndexEntry indexEntry,
        ElasticIndexSettings indexSettings)
    {
        Map = indexMap;
        IndexEntry = indexEntry;
        IndexSettings = indexSettings;
    }

    public ElasticsearchIndexMap Map { get; }

    public DocumentIndexEntry IndexEntry { get; }

    public ElasticIndexSettings IndexSettings { get; }
}
