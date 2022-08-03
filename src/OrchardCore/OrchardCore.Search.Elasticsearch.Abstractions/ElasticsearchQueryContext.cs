using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchQueryContext
    {
        public ElasticsearchQueryContext()
        {

        }

        public ElasticsearchQueryContext(string indexName)
        {
            IndexName = indexName;
        }

        public string IndexName { get; set; }

        /// <summary>
        /// The Analyzer later needs to be implemented from a list of Elasticsearch Analyzers
        /// </summary>
        public IAnalyzer DefaultAnalyzer { get; }

        //We may need to have configurable index searcher
        //public IndexSearcher IndexSearcher { get; set; }
    }
}
