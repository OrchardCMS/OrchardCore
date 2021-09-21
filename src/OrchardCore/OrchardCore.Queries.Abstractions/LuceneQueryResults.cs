using System;
using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Queries
{
    [Obsolete("Moved to OrchardCore.Lucene.Abstractions", false)]
    public class LuceneQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
