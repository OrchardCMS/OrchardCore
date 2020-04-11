using Lucene.Net.Search;

namespace OrchardCore.Lucene
{
    public class LuceneTopDocs
    {
        public TopDocs TopDocs { get; set; }
        public int Count { get; set; }
    }
}
