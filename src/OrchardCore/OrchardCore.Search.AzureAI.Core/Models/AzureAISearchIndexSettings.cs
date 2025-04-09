using OrchardCore.Entities;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettings : Entity
{
    public string Id { get; set; }

    public string Source { get; set; }

    public string IndexName { get; set; }

    public string IndexFullName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public IList<AzureAISearchIndexMap> IndexMappings { get; init; } = [];

    [Obsolete("This property will be removed in a future release.")]
    public long GetLastTaskId()
        => this.As<ContentIndexingMetadata>().LastTaskId;

    [Obsolete("This property will be removed in a future release.")]
    public void SetLastTaskId(long lastTaskId)
        => this.Alter<ContentIndexingMetadata>(metadata =>
        {
            metadata.LastTaskId = lastTaskId;
        });

    // The dictionary key should be indexingKey Not AzureFieldKey.
    public Dictionary<string, IEnumerable<AzureAISearchIndexMap>> GetMaps()
        => IndexMappings.GroupBy(group => group.IndexingKey)
        .ToDictionary(group => group.Key, group => group.Select(map => map));
}
