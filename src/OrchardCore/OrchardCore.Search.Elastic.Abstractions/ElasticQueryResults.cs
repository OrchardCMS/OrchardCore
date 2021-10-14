using OrchardCore.Queries;
using System.Collections.Generic;

namespace OrchardCore.Search.Elastic
{
    public class ElasticQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
