using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public class SQLQueryResults: IQueryResults<object>
    {
        public IEnumerable<object> Items { get; set; }
    }
}
