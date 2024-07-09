namespace OrchardCore.Queries;

public abstract class QueryContextBase
{
    public Query Query { get; }

    public QueryContextBase(Query query)
    {
        Query = query;
    }
}
