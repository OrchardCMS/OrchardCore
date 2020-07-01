using System.Collections.Generic;

namespace OrchardCore.Lucene
{
    public class LuceneOptions
    {
        public IList<ILuceneAnalyzer> Analyzers { get; } = new List<ILuceneAnalyzer>();
    }
}
