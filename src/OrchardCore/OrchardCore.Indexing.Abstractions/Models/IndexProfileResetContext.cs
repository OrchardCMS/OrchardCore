namespace OrchardCore.Indexing.Models;

public sealed class IndexProfileResetContext
{
    public IndexProfile IndexProfile { get; }

    public IndexProfileResetContext(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        IndexProfile = indexProfile;
    }
}
