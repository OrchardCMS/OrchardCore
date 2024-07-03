using YesSql.Indexes;

namespace OrchardCore.Queries.Indexes;

public class QueryIndex : MapIndex
{
    public string Name { get; set; }

    public string Source { get; set; }
}

public class QueryIndexProvider : IndexProvider<Query>
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
