using System.Collections.Generic;

namespace OrchardCore.Queries.Sql
{
    public class SQLQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
