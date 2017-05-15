using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;

namespace Orchard.Lucene.Services
{
    /// <summary>
    /// Coordinates <see cref="ILuceneAnalyzerProvider"/> implementations
    /// to return the list of all available <see cref="ILuceneAnalyzer"/> objects.
    /// </summary>
    public class LuceneAnalyzerManager
    {
        /// <summary>
        /// We lock this dictionary on <see cref="RegisterAnalyzer"/> and <see cref="RemoveAnalyzer"/> only
        /// as we assume it can only be changed on application startup and is readonly at runtime.
        /// </summary>
        private readonly Dictionary<string, ILuceneAnalyzer> _analyzers;

        public LuceneAnalyzerManager()
        {
            _analyzers = new Dictionary<string, ILuceneAnalyzer>(StringComparer.OrdinalIgnoreCase);
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

        /// <summary>
        /// Registers a named lucene analyzer.
        /// This method can only be called during initilization of the application.
        /// </summary>
        public void RegisterAnalyzer(ILuceneAnalyzer analyzer)
        {
            lock (_analyzers)
            {
                _analyzers[analyzer.Name] = analyzer;
            }
        }

        /// <summary>
        /// Removes a named lucene analyzer.
        /// This method can only be called during initilization of the application.
        /// </summary>
        public void RemoveAnalyzer(string name)
        {
            lock (_analyzers)
            {
                _analyzers.Remove(name);
            }
        }
    }
}
