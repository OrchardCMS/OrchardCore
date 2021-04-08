using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace OrchardCore.Search.Elastic
{
    public class ElasticQueryContext
    {
        public ElasticQueryContext()
        {
            
        }

        public ElasticQueryContext(string indexName)
        {
            IndexName = indexName;
        }

        public string IndexName { get; set; }

        /// <summary>
        /// The Analyzer later needs to be implemented from a list of Elastic Analyzers and Lucene reference needs to be removed
        /// </summary>
        public Analyzer DefaultAnalyzer { get; }

        //We may need to have configurable index searcher
        //public IndexSearcher IndexSearcher { get; set; }
    }
}
