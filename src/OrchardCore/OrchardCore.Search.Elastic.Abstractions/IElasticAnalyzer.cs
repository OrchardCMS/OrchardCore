using Lucene.Net.Analysis;

namespace OrchardCore.Search.Elastic
{
    /// <summary>
    /// Represents an <see cref="Analyzer"/> instance that is available in the system.
    /// This is not being used in case of Elastic, We will need to implement it later
    /// </summary>
    public interface IElasticAnalyzer
    {
        string Name { get; }
        Analyzer CreateAnalyzer();
    }
}
