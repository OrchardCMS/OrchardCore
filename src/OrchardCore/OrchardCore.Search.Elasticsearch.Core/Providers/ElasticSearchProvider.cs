using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Core.Providers
{
    /// <summary>
    /// Creates an Elasticsearch SearchProvider.
    /// </summary>
    internal class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elasticsearch") { }
    }
}
