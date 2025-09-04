namespace OrchardCore.Queries;

public sealed class LoadedQueryContext : QueryContextBase
{
    public LoadedQueryContext(Query query)
        : base(query)
    {
    }
}
