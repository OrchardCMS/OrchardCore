namespace OrchardCore.Queries;

public class QueryContextBase
{
    public Query Query { get; }

    public QueryContextBase(Query query)
    {
        Query = query;
    }
}
