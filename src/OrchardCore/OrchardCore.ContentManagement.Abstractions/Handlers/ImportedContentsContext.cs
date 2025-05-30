namespace OrchardCore.ContentManagement.Handlers;

public sealed class ImportedContentsContext
{
    public IEnumerable<ContentItem> OriginalItems { get; }

    public IEnumerable<ContentItem> ImportedItems { get; }

    public ImportedContentsContext(IEnumerable<ContentItem> imported, IEnumerable<ContentItem> original)
    {
        ArgumentNullException.ThrowIfNull(imported);
        ArgumentNullException.ThrowIfNull(original);

        ImportedItems = imported;
        OriginalItems = original;
    }
}
