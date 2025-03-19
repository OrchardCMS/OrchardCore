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

    private long _lastTaskId;

    public long GetLastTaskId()
        => _lastTaskId;

    public void SetLastTaskId(long lastTaskId)
        => _lastTaskId = lastTaskId;

    // The dictionary key should be indexingKey Not AzureFieldKey.
    public Dictionary<string, IEnumerable<AzureAISearchIndexMap>> GetMaps()
        => IndexMappings.GroupBy(group => group.IndexingKey)
        .ToDictionary(group => group.Key, group => group.Select(map => map));
}
