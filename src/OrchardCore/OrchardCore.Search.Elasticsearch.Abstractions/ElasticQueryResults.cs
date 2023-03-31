using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public long Count { get; set; }
    }
}
