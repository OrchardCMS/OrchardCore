using System.Text.Json.Serialization;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class CognitiveSearchIndexSettings
{
    [JsonIgnore]
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }
}
