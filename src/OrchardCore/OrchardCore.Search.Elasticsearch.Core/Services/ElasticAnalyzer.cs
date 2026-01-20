using System;
using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    public class ElasticAnalyzer : IElasticAnalyzer
    {
        private readonly Func<IAnalyzer> _factory;

        public ElasticAnalyzer(string name, Func<IAnalyzer> factory)
        {
            _factory = factory;
            Name = name;
        }

        public ElasticAnalyzer(string name, IAnalyzer instance) : this(name, () => instance)
        {
        }

        public string Name { get; }

        public IAnalyzer CreateAnalyzer()
        {
            return _factory();
        }
    }
}
