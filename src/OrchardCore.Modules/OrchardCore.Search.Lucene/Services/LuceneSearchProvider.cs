using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Lucene.Services
{
    /// <summary>
    /// Creates a Lucene SearchProvider.
    /// </summary>
    internal class LuceneSearchProvider : SearchProvider
    {
        public LuceneSearchProvider() : base("Lucene")
        {
            AreaName = GetType().Assembly.GetName().Name;
        }
    }
}
