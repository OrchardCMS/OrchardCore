using System;
using Lucene.Net.Analysis;

namespace OrchardCore.Search.Lucene.Services
{
    public class LuceneAnalyzer : ILuceneAnalyzer
    {
        private readonly Func<Analyzer> _factory;

        public LuceneAnalyzer(string name, Func<Analyzer> factory)
        {
            _factory = factory;
            Name = name;
        }

        public LuceneAnalyzer(string name, Analyzer instance) : this(name, () => instance)
        {
        }

        public string Name { get; }
        public Analyzer CreateAnalyzer()
        {
            return _factory();
        }
    }
}
