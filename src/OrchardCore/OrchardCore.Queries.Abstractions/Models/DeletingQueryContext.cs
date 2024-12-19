namespace OrchardCore.Queries;

public sealed class DeletingQueryContext : QueryContextBase
{
    public DeletingQueryContext(Query query)
        : base(query)
    {
    }
}
