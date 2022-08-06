using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Lucene.Services
{
    /// <summary>
    /// Provides a way to determine that a Lucene SearchProvider is available to the current tenant.
    /// </summary>
    internal class LuceneSearchProvider : SearchProvider
    {
        public LuceneSearchProvider() : base("Lucene")
        {
        }
    }
}
