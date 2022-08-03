namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticsearchQueryViewModel
    {
        public string[] Indices { get; set; }
        public string Index { get; set; }
        public string Query { get; set; }
        public bool ReturnContentItems { get; set; }
    }
}
