using System.Collections.Generic;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchOptions
    {
        public IList<IElasticsearchAnalyzer> Analyzers { get; } = new List<IElasticsearchAnalyzer>();
    }
}
