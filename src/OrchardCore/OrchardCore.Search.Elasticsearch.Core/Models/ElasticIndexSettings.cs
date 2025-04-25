using OrchardCore.Data.Documents;
using OrchardCore.Search.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticIndexSettings : IndexSettingsBase
{
    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public bool StoreSourceData { get; set; } = true;
}

public class ElasticIndexSettingsDocument : Document
{
    public Dictionary<string, ElasticIndexSettings> ElasticIndexSettings { get; set; } = [];
}
