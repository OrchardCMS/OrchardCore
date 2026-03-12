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
        return string.IsNullOrEmpty(AnalyzerName)
            ? ElasticsearchConstants.DefaultAnalyzer
            : AnalyzerName;
    }

    public string GetQueryAnalyzerName()
    {
        return AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
    }
}
