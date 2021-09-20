using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Data
{
    public class SQLQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
    }
}
