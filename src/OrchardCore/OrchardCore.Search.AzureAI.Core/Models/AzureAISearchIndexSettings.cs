using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettings
{
    [JsonIgnore]
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public IList<AzureAISearchIndexMap> IndexMappings { get; set; }

    private long _lastTaskId = 0;

    public long GetLastTaskId()
        => _lastTaskId;

    public void SetLastTaskId(long lastTaskId)
        => _lastTaskId = lastTaskId;
}
