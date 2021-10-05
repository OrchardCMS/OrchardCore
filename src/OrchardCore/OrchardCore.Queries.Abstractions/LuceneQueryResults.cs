using System;
using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Queries
{
    [Obsolete("This class has been deprecated and we will be removed in the next major release, please use OrchardCore.Lucene.Abstractions instead.", false)]
    public class LuceneQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
