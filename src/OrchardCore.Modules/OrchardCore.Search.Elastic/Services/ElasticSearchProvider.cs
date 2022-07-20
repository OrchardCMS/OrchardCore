using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elastic.Services
{
    internal class ElasticSearchProvider : ISearchProvider
    {
        public string Name { get; } = "Elastic";
    }
}
