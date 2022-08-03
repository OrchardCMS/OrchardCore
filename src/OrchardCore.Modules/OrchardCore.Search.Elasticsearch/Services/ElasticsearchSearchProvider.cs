using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Services
{
    internal class ElasticsearchSearchProvider : SearchProvider
    {
        public ElasticsearchSearchProvider() : base("Elasticsearch")
        {
        }
    }
}
