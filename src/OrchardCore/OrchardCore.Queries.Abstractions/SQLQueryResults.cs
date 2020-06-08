using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public class SQLQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
