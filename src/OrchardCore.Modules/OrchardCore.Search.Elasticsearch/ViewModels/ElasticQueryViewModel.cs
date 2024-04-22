namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticQueryViewModel
    {
        public string[] Indices { get; set; }
        public string Index { get; set; }
        public string Query { get; set; }
        public bool ReturnContentItems { get; set; }
    }
}
