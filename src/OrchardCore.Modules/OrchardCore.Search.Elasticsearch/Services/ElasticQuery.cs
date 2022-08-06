using OrchardCore.Queries;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticQuery : Query
    {
        public ElasticQuery() : base("Elasticsearch") { }

        public string Index { get; set; }

        public string Template { get; set; }

        public bool ReturnContentItems { get; set; }
    }
}
