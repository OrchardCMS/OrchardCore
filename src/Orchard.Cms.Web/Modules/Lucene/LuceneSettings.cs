using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Lucene
{
    public class LuceneSettings
    {
        // TODO: Load settings from database

        public Analyzer GetAnalyzer()
        {
            return new StandardAnalyzer(GetVersion());
        }

        public string GetDefaultSearchIndex()
        {
            return "Search";
        }

        public LuceneVersion GetVersion()
        {
            return LuceneVersion.LUCENE_48;
        }

        public string[] GetSearchFields()
        {
            return new string[] { "Content.ContentItemMetadata.DisplayText", "Content.BodyAspect.Body" };
        }
    }
}
