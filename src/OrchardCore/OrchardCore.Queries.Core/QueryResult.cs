using System.Collections.Generic;

namespace OrchardCore.Queries.Core;

public sealed class QueryResult
{
    public IEnumerable<Query> Records { get; set; }

    public int Count { get; set; }
}
