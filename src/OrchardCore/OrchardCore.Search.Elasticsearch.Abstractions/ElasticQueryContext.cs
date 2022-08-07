using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticQueryContext
    {
        public ElasticQueryContext(string indexName, IAnalyzer defaultAnalyzer)
        {
            IndexName = indexName;
            DefaultAnalyzer = defaultAnalyzer;
        }

        public string IndexName { get; set; }
        public IAnalyzer DefaultAnalyzer { get; }

        //We may need to have configurable index searcher
        //public IndexSearcher IndexSearcher { get; set; }
    }
}
