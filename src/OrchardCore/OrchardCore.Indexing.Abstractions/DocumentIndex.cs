namespace OrchardCore.Indexing;

public class DocumentIndex : DocumentIndexBase
{
    public string ContentItemId { get; }

    public string ContentItemVersionId { get; }

    public DocumentIndex(string contentItemId, string contentItemVersionId)
    {
        ContentItemId = contentItemId;
        ContentItemVersionId = contentItemVersionId;
    }
}
