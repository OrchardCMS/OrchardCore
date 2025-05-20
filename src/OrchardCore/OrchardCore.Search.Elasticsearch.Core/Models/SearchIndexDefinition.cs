using Elastic.Clients.Elasticsearch.Mapping;
using OrchardCore.Search.Elasticsearch.Core.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class SearchIndexDefinition
{
    public SearchIndexDefinition(
        TypeMapping typeMapping,
        DocumentIndexEntry indexEntry,
        ElasticIndexSettings indexSettings)
    {
        Map = typeMapping;
        IndexEntry = indexEntry;
        IndexSettings = indexSettings;
    }

    public TypeMapping Map { get; }

    public DocumentIndexEntry IndexEntry { get; }

    public ElasticIndexSettings IndexSettings { get; }
}
