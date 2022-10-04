using System;
using System.Collections.Generic;

namespace OrchardCore.Queries
{
    [Obsolete("This class has been deprecated and we will be removed in the next major release, please use OrchardCore.Search.Lucene.Abstractions instead.", false)]
    public class LuceneQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
