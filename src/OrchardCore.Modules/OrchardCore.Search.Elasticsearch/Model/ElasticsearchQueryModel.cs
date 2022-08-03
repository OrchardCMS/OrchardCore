namespace OrchardCore.Search.Elasticsearch.Model
{
    public class ElasticsearchQueryModel
    {
        public string IndexName { set; get; }
        public string Query { set; get; }
        public string Parameters { set; get; }
    }
}
