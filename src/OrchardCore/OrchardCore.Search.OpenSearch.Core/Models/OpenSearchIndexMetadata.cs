using System.Text.Json.Serialization;
using OrchardCore.Search.OpenSearch.Core.Json;
using OrchardCore.Search.OpenSearch.Models;

namespace OrchardCore.Search.OpenSearch.Core.Models;

[JsonConverter(typeof(OpenSearchIndexMetadataConverter))]
public sealed class OpenSearchIndexMetadata
{
    public bool StoreSourceData { get; set; } = true;

    public string AnalyzerName { get; set; }

    [JsonConverter(typeof(OpenSearchIndexMapConverter))]
    public OpenSearchIndexMap IndexMappings { get; set; }

    public string GetAnalyzerName()
    {
        return string.IsNullOrEmpty(AnalyzerName)
            ? OpenSearchConstants.DefaultAnalyzer
            : AnalyzerName;
    }

    public string GetQueryAnalyzerName()
    {
        return AnalyzerName ?? OpenSearchConstants.DefaultAnalyzer;
    }
}
