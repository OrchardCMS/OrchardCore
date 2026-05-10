using OrchardCore.Queries;

namespace OrchardCore.OpenSearch;

public class OpenSearchQueryResults : IQueryResults
{
    public IEnumerable<object> Items { get; set; }
    public long Count { get; set; }
}
