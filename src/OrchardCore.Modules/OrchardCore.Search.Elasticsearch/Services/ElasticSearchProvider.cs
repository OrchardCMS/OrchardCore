using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Services
{
    internal class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elasticsearch") { }
    }
}
