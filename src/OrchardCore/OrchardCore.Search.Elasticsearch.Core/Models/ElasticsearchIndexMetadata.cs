using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public sealed class ElasticsearchIndexMetadata
{
    public string KeyFieldName { get; set; }

    public string AnalyzerName { get; set; }

    public ElasticsearchIndexMap IndexMappings { get; set; }

    public string GetAnalyzerName()
    {
        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        return AnalyzerName == "standardanalyzer"
            ? ElasticsearchConstants.DefaultAnalyzer
            : AnalyzerName;
    }

    public string GetQueryAnalyzerName()
    {
        return AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
    }
}
