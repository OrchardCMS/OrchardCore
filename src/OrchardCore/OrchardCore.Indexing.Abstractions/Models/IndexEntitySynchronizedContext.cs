namespace OrchardCore.Indexing.Models;

public sealed class IndexEntitySynchronizedContext
{
    public IndexEntity Index { get; }

    public IndexEntitySynchronizedContext(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        Index = index;
    }
}
