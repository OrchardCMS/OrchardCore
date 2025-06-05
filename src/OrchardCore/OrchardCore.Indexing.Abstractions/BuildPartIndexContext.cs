using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing;

public class BuildPartIndexContext : BuildDocumentIndexContext
{
    public BuildPartIndexContext(
        ContentItemDocumentIndex documentIndex,
        ContentItem contentItem,
        IList<string> keys,
        ContentTypePartDefinition typePartDefinition,
        IContentIndexSettings settings)
        : base(documentIndex, contentItem, keys, settings)
    {
        ContentTypePartDefinition = typePartDefinition;
    }

    public ContentTypePartDefinition ContentTypePartDefinition { get; }
}
