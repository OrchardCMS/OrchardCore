using OrchardCore.Indexing;

namespace OrchardCore.Search.Lucene;

public sealed class LuceneIndexNameProvider : IIndexNameProvider
{
    public string GetFullIndexName(string name)
        => name;
}
