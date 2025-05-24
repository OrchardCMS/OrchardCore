namespace OrchardCore.Indexing.Models;

public class IndexRebuildContext
{
    public IndexEntity Index { get; }

    public IndexRebuildContext(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        Index = index;
    }
}
