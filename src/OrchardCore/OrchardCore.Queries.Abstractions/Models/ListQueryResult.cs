namespace OrchardCore.Queries;

public sealed class ListQueryResult
{
    public IEnumerable<Query> Records { get; set; }

    public int Count { get; set; }
}
