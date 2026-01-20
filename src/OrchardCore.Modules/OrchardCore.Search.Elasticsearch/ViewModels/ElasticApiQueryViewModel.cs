namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticApiQueryViewModel
    {
        public string IndexName { set; get; }
        public string Query { set; get; }
        public string Parameters { set; get; }
    }
}
