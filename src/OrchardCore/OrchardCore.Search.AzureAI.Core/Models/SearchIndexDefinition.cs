using OrchardCore.Indexing.Models;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Models;

public sealed class SearchIndexDefinition
{
    public SearchIndexDefinition(
        AzureAISearchIndexMap indexMap,
        DocumentIndexEntry indexEntry,
        IndexProfile index)
    {
        Map = indexMap;
        IndexEntry = indexEntry;
        Index = index;
    }

    public AzureAISearchIndexMap Map { get; }

    public DocumentIndexEntry IndexEntry { get; }

    public IndexProfile Index { get; }
}
