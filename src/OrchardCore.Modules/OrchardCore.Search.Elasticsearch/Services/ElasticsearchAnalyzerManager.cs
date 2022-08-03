using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Nest;

namespace OrchardCore.Search.Elasticsearch.Services
{
    /// <summary>
    /// Coordinates <see cref="IElasticsearchAnalyzer"/> implementations provided by <see cref="ElasticsearchOptions"/>
    /// to return the list of all available <see cref="IElasticsearchAnalyzer"/> objects.
    /// </summary>
    public class ElasticsearchAnalyzerManager
    {
        private readonly Dictionary<string, IElasticsearchAnalyzer> _analyzers;

        public ElasticsearchAnalyzerManager(IOptions<ElasticsearchOptions> options)
        {
            _analyzers = new Dictionary<string, IElasticsearchAnalyzer>(StringComparer.OrdinalIgnoreCase);

            foreach (var analyzer in options.Value.Analyzers)
            {
                _analyzers[analyzer.Name] = analyzer;
            }
        }

        public IEnumerable<IElasticsearchAnalyzer> GetAnalyzers()
        {
            return _analyzers.Values;
        }

        public IAnalyzer CreateAnalyzer(string name)
        {
            if (_analyzers.TryGetValue(name, out var analyzer))
            {
                return analyzer.CreateAnalyzer();
            }

            return null;
        }
    }
}
