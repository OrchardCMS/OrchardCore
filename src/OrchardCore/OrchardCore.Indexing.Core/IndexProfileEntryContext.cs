using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public sealed class IndexProfileEntryContext
{
    public readonly IndexProfile IndexProfile;

    public long LastTaskId { get; }

    public readonly IDocumentIndexManager DocumentIndexManager;

    public IndexProfileEntryContext(IndexProfile indexProfile, IDocumentIndexManager documentIndexManager, long lastTaskId)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentNullException.ThrowIfNull(documentIndexManager);

        IndexProfile = indexProfile;
        DocumentIndexManager = documentIndexManager;
        LastTaskId = lastTaskId;
    }
}
