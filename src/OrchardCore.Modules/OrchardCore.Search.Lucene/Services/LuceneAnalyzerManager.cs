using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Microsoft.Extensions.Options;

namespace OrchardCore.Search.Lucene.Services
{
    /// <summary>
    /// Coordinates <see cref="ILuceneAnalyzer"/> implementations provided by <see cref="LuceneOptions"/>
    /// to return the list of all available <see cref="ILuceneAnalyzer"/> objects.
    /// </summary>
    public class LuceneAnalyzerManager
    {
        private readonly Dictionary<string, ILuceneAnalyzer> _analyzers;

        public LuceneAnalyzerManager(IOptions<LuceneOptions> options)
        {
            _analyzers = new Dictionary<string, ILuceneAnalyzer>(StringComparer.OrdinalIgnoreCase);

            foreach (var analyzer in options.Value.Analyzers)
            {
                _analyzers[analyzer.Name] = analyzer;
            }
        }

        public IEnumerable<ILuceneAnalyzer> GetAnalyzers()
        {
            return _analyzers.Values;
        }

        public Analyzer CreateAnalyzer(string name)
        {
            if (_analyzers.TryGetValue(name, out var analyzer))
            {
                return analyzer.CreateAnalyzer();
            }

            return null;
        }
    }
}
