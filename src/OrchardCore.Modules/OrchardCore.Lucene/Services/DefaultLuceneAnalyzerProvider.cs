using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace OrchardCore.Lucene.Services
{
    public class DefaultLuceneAnalyzerProvider : ILuceneAnalyzerProvider
    {
        public string Key { get; }

        public string AnalyzerName { get; } = "standardanalyzer";


        public LuceneVersion Version { get; } = LuceneVersion.LUCENE_48;

        public ILuceneAnalyzer LuceneAnalyzer()
        {
            return new LuceneAnalyzer(AnalyzerName, new StandardAnalyzer(Version));
        }
    }
}
