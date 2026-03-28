using OrchardCore.Queries;

namespace OrchardCore.Search.OpenSearch;

public class OpenSearchQueryResults : IQueryResults
{
    public IEnumerable<object> Items { get; set; }
    public long Count { get; set; }
}
