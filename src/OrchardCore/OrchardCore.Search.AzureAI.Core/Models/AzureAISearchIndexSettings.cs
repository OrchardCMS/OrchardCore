namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettings
{
    public string IndexName { get; set; }

    public string IndexFullName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public IList<AzureAISearchIndexMap> IndexMappings { get; set; }

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
