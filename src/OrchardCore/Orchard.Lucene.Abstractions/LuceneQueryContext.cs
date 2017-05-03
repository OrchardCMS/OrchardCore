using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace Orchard.Lucene
{
    public class LuceneQueryContext
    {
        public LuceneQueryContext(LuceneVersion defaultVersion, Analyzer defaultAnalyzer)
        {
            DefaultAnalyzer = defaultAnalyzer;
            DefaultVersion = defaultVersion;
        }

        public LuceneVersion DefaultVersion { get; } = LuceneVersion.LUCENE_48;
        public Analyzer DefaultAnalyzer { get; }
    }
}
