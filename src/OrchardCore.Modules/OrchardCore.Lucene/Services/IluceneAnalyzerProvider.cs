using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Util;

namespace OrchardCore.Lucene.Services
{
    public interface ILuceneAnalyzerProvider
    {
        /// <summary>
        /// Use the tenant's culture
        /// </summary>
        string Key { get; }
        LuceneVersion Version { get; }
        string AnalyzerName { get; }
        ILuceneAnalyzer LuceneAnalyzer();
    }
}
