using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public class SQLQueryResult : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
