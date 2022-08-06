using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Core.Providers
{
    internal class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elasticsearch") { }
    }
}
