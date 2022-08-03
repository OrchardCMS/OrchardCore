using Nest;

namespace OrchardCore.Search.Elasticsearch
{
    /// <summary>
    /// Represents an <see cref="IAnalyzer"/> instance that is available in the system.
    /// </summary>
    public interface IElasticsearchAnalyzer
    {
        string Name { get; }
        IAnalyzer CreateAnalyzer();
    }
}
