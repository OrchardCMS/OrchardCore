using YesSql.Indexes;

namespace OrchardCore.Queries.Indexes;

public sealed class QueryIndex : MapIndex
{
    public string Name { get; set; }

    public string Source { get; set; }
}

public sealed class QueryIndexProvider : IndexProvider<Query>
{
    public override void Describe(DescribeContext<Query> context)
    {
        context
            .For<QueryIndex>()
            .Map(x => new QueryIndex
            {
                Name = x.Name,
                Source = x.Source,
            });
    }
}
