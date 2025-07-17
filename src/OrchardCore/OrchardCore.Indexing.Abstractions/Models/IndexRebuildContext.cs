namespace OrchardCore.Indexing.Models;

public class IndexRebuildContext
{
    public IndexProfile IndexProfile { get; }

    public IndexRebuildContext(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        IndexProfile = indexProfile;
    }
}
