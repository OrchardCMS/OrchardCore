using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;

namespace OrchardCore.Lucene.Services
{
    public class LuceneAnalyzerProviderManager : ILuceneAnalyzerProviderManager
    {
        private readonly IEnumerable<ILuceneAnalyzerProvider> _luceneAnalyzerProviders;
        public LuceneAnalyzerProviderManager(IEnumerable<ILuceneAnalyzerProvider> luceneAnalyzerProviders)
        {
            _luceneAnalyzerProviders = luceneAnalyzerProviders;
        }

        public ILuceneAnalyzerProvider GetLuceneAnalyzerProvider(string key = "")
        {
            if (string.IsNullOrEmpty(key))
            {
                return new DefaultLuceneAnalyzerProvider();
            }
            foreach (var luceneAnalyzerProvider in _luceneAnalyzerProviders)
            {
                if (string.Equals(luceneAnalyzerProvider.Key, key, StringComparison.InvariantCultureIgnoreCase))
                {
                    return luceneAnalyzerProvider;
                }
            }
            return new DefaultLuceneAnalyzerProvider();
        }
       
    }
}
