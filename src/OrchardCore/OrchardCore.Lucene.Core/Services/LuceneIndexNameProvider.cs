using OrchardCore.Indexing;

namespace OrchardCore.Lucene;

public sealed class LuceneIndexNameProvider : IIndexNameProvider
{
    public string GetFullIndexName(string name)
        => name;
}
