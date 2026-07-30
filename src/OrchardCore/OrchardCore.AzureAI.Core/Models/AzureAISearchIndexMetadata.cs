namespace OrchardCore.AzureAI.Models;

public class AzureAISearchIndexMetadata
{
    public string AnalyzerName { get; set; }

    public IList<AzureAISearchIndexMap> IndexMappings { get; init; } = [];

    public VectorSearchMappings VectorSearchMappings { get; set; }

    // The dictionary key should be indexingKey Not AzureFieldKey.
    public Dictionary<string, IEnumerable<AzureAISearchIndexMap>> GetMaps()
        => IndexMappings.GroupBy(group => group.IndexingKey)
        .ToDictionary(group => group.Key, group => group.Select(map => map));
}
