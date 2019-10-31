using System.Collections.Generic;

namespace OrchardCore.Queries
{
    public class LuceneQueryResults: IQueryResults<object>
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
