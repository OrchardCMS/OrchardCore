using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Lucene.Services
{
    internal class LuceneSearchProvider : SearchProvider
    {
        public LuceneSearchProvider() : base("Lucene")
        {
        }
    }
}
