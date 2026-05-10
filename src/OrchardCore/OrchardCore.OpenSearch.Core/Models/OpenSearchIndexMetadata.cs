using System.Text.Json.Serialization;
using OrchardCore.OpenSearch.Core.Json;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Models;

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
