using OrchardCore.ContentManagement;

namespace OrchardCore.Indexing;

public class BuildIndexContext
{
    public BuildIndexContext(
        ContentItemDocumentIndex documentIndex,
        ContentItem contentItem,
        IList<string> keys,
        IContentIndexSettings settings
    )
    {
        ContentItem = contentItem;
        DocumentIndex = documentIndex;
        Keys = keys;
        Settings = settings;
    }

    public IList<string> Keys { get; }

    public ContentItem ContentItem { get; }

    public ContentItemDocumentIndex DocumentIndex { get; }

    public IContentIndexSettings Settings { get; }
}
