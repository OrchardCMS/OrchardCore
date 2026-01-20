namespace OrchardCore.Queries;

public sealed class DeletedQueryContext : QueryContextBase
{
    public DeletedQueryContext(Query query)
        : base(query)
    {
    }
}
