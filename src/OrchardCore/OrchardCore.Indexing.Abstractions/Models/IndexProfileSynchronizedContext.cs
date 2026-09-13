namespace OrchardCore.Indexing.Models;

public sealed class IndexProfileSynchronizedContext
{
    public IndexProfile IndexProfile { get; }

    /// <summary>
    /// Gets whether the shared indexing processor has already completed this synchronization.
    /// Handlers may perform their extension work, but must not repeat completed indexing.
    /// </summary>
    public bool IsIndexingCompleted { get; init; }

    public IndexProfileSynchronizedContext(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        IndexProfile = indexProfile;
    }
}
