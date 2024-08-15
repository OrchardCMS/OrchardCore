namespace OrchardCore.Queries;

public sealed class InitializedQueryContext : QueryContextBase
{
    public InitializedQueryContext(Query query)
        : base(query)
    {
    }
}
