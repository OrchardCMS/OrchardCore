using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Providers
{
    /// <summary>
    /// Creates an Elasticsearch SearchProvider.
    /// </summary>
    public class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elasticsearch")
        {
            AreaName = GetType().Assembly.GetName().Name;
        }
    }
}
