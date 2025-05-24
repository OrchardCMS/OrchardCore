namespace OrchardCore.Indexing.Models;

public sealed class IndexEntityResetContext
{
    public IndexEntity Index { get; }

    public IndexEntityResetContext(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        Index = index;
    }
}
