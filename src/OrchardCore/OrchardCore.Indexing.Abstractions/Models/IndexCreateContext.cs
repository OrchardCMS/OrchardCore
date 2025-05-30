namespace OrchardCore.Indexing.Models;

public class IndexCreateContext
{
    public IndexEntity Index { get; }

    public IndexCreateContext(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        Index = index;
    }
}
