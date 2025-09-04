using System.Text.Json.Serialization;
using OrchardCore.Search.Elasticsearch.Core.Json;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

[JsonConverter(typeof(ElasticsearchIndexMetadataConverter))]
public sealed class ElasticsearchIndexMetadata
{
    public bool StoreSourceData { get; set; } = true;

    public string AnalyzerName { get; set; }

    [JsonConverter(typeof(ElasticsearchIndexMapConverter))]
    public ElasticsearchIndexMap IndexMappings { get; set; }

    public string GetAnalyzerName()
    {
        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        return AnalyzerName == "standardanalyzer" || string.IsNullOrEmpty(AnalyzerName)
            ? ElasticsearchConstants.DefaultAnalyzer
            : AnalyzerName;
    }

    public string GetQueryAnalyzerName()
    {
        return AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
    }
}
