using System;
using Lucene.Net.Analysis;

namespace OrchardCore.Search.Elastic.Services
{
    /// <summary>
    /// All Lucene related analyzers needs to replaced
    /// </summary>
    public class ElasticAnalyzer : IElasticAnalyzer
    {
        private readonly Func<Analyzer> _factory;

        public ElasticAnalyzer(string name, Func<Analyzer> factory)
        {
            _factory = factory;
            Name = name;
        }

        public ElasticAnalyzer(string name, Analyzer instance) : this(name, () => instance)
        {
        }

        public string Name { get; }
        public Analyzer CreateAnalyzer()
        {
            return _factory();
        }
    }
}
