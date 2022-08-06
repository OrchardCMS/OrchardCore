using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Core.Providers
{
    /// <summary>
    /// Provides a way to determine that an Elasticsearch SearchProvider is available to the current tenant.
    /// </summary>
    internal class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elasticsearch") { }
    }
}
