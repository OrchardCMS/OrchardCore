using System;
using Nest;

namespace OrchardCore.Search.Elasticsearch.Services
{
    public class ElasticsearchAnalyzer : IElasticsearchAnalyzer
    {
        private readonly Func<IAnalyzer> _factory;

        public ElasticsearchAnalyzer(string name, Func<IAnalyzer> factory)
        {
            _factory = factory;
            Name = name;
        }

        public ElasticsearchAnalyzer(string name, IAnalyzer instance) : this(name, () => instance)
        {
        }

        public string Name { get; }

        public IAnalyzer CreateAnalyzer()
        {
            return _factory();
        }
    }
}
