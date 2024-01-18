using Lucene.Net.Analysis;

namespace OrchardCore.Search.Lucene
{
    /// <summary>
    /// Represents an <see cref="Analyzer"/> instance that is available in the system.
    /// </summary>
    public interface ILuceneAnalyzer
    {
        string Name { get; }
        Analyzer CreateAnalyzer();
    }
}
