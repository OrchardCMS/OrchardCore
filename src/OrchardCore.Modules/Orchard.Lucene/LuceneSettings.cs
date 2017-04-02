using Lucene.Net.Util;

namespace Orchard.Lucene
{
    public class LuceneSettings
    {
        public static LuceneVersion DefaultVersion = LuceneVersion.LUCENE_48;

        public string SearchIndex { get; set; }

        public string[] SearchFields { get; set; } = new string[0];
    }
}
