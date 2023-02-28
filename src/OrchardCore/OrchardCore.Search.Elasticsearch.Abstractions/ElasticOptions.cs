using System.Collections.Generic;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticOptions
    {
        public IList<IElasticAnalyzer> Analyzers { get; } = new List<IElasticAnalyzer>();
    }
}
