using OrchardCore.Queries;

namespace OrchardCore.Lucene
{
    public class LuceneQuery : Query
    {
        public LuceneQuery() : base("Lucene")
        {
        }

        public string Index { get; set; }
        public string Template { get; set; }
        public bool ReturnContentItems { get; set; }
    }
}
