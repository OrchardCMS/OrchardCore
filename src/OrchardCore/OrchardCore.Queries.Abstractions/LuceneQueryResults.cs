using System;
using System.Collections.Generic;

namespace OrchardCore.Queries
{
    [Obsolete("Replaced by SearchEngineQueryResult", false)]
    public class LuceneQueryResults : IQueryResults {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
