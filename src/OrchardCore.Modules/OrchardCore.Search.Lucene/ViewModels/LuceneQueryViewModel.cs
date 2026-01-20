namespace OrchardCore.Search.Lucene.ViewModels
{
    public class LuceneQueryViewModel
    {
        public string[] Indices { get; set; }
        public string Index { get; set; }
        public string Query { get; set; }
        public bool ReturnContentItems { get; set; }
    }
}
