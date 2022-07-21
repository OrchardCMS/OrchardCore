using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elastic.Services
{
    internal class ElasticSearchProvider : SearchProvider
    {
        public ElasticSearchProvider() : base("Elastic")
        {
        }
    }
}
