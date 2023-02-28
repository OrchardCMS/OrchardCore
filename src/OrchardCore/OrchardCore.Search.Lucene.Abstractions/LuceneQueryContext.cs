using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace OrchardCore.Search.Lucene
{
    public class LuceneQueryContext
    {
        public LuceneQueryContext(IndexSearcher indexSearcher, LuceneVersion defaultVersion, Analyzer defaultAnalyzer)
        {
            DefaultAnalyzer = defaultAnalyzer;
            DefaultVersion = defaultVersion;
            IndexSearcher = indexSearcher;
        }

        public LuceneVersion DefaultVersion { get; } = LuceneVersion.LUCENE_48;
        public Analyzer DefaultAnalyzer { get; }
        public IndexSearcher IndexSearcher { get; set; }
    }
}
