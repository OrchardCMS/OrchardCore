using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAIIndexSettings
{
    [JsonIgnore]
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public IList<AzureAIIndexMap> IndexMappings { get; set; }
}
