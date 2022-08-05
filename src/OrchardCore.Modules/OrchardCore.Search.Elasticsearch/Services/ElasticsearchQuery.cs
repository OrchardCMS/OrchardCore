using OrchardCore.Queries;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchQuery : Query
    {
        public ElasticsearchQuery() : base("Elasticsearch") { }

        public string Index { get; set; }

        public string Template { get; set; }

        public bool ReturnContentItems { get; set; }
    }
}
