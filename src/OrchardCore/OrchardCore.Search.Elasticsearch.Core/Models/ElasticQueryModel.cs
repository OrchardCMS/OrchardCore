namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ElasticQueryModel
    {
        public string IndexName { set; get; }
        public string Query { set; get; }
        public string Parameters { set; get; }
    }
}
