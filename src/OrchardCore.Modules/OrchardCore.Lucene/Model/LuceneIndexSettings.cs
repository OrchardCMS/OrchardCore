namespace OrchardCore.Lucene.Model
{
    public class LuceneIndexSettings
    {
        public string IndexName { get; set; }

        public string AnalyzerName { get; set; }

        public bool IndexLatest { get; set; }

        public string[] IndexedContentTypes { get; set; }
    }
}
