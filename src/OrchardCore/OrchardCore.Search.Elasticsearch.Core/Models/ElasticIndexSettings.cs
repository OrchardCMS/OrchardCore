using Elastic.Clients.Elasticsearch.Mapping;
using OrchardCore.Search.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticIndexSettings : IndexSettingsBase
{
    public string Id { get; set; }

    public string Source { get; set; }

    public string AnalyzerName { get; set; }

    public string QueryAnalyzerName { get; set; }

    public string IndexFullName { get; set; }

    public TypeMapping IndexMappings { get; init; } = new TypeMapping();
}
