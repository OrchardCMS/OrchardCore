using System.Collections.Generic;
using OrchardCore.Queries;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchQueryResults : IQueryResults
    {
        public IEnumerable<object> Items { get; set; }
        public int Count { get; set; }
    }
}
