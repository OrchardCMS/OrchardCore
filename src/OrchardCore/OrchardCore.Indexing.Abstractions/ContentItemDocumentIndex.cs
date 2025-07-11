namespace OrchardCore.Indexing;

/// <summary>
/// Represents a document index for a content item, which includes the content item ID and version ID.
/// </summary>
public class ContentItemDocumentIndex : DocumentIndex
{
    public string ContentItemId { get; }

    public string ContentItemVersionId { get; }

    public ContentItemDocumentIndex(string contentItemId, string contentItemVersionId)
        : base(contentItemId)
    {
        ContentItemId = contentItemId;
        ContentItemVersionId = contentItemVersionId;
    }
}
