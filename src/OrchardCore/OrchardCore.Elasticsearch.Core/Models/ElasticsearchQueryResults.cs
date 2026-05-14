using OrchardCore.Queries;

namespace OrchardCore.Elasticsearch;

public class ElasticsearchQueryResults : IQueryResults
{
    public IEnumerable<object> Items { get; set; }
    public long Count { get; set; }
}
