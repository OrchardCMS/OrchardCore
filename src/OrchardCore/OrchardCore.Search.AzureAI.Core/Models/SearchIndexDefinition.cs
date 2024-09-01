using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Models;

public sealed class SearchIndexDefinition
{
    public SearchIndexDefinition(
        AzureAISearchIndexMap indexMap,
        DocumentIndexEntry indexEntry,
        AzureAISearchIndexSettings indexSettings)
    {
        Map = indexMap;
        IndexEntry = indexEntry;
        IndexSettings = indexSettings;
    }

    public AzureAISearchIndexMap Map { get; }

    public DocumentIndexEntry IndexEntry { get; }

    public AzureAISearchIndexSettings IndexSettings { get; }
}
