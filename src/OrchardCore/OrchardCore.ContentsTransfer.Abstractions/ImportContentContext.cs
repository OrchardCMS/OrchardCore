using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer;

public class ImportContentContext
{
    public ContentItem ContentItem { get; set; }

    public ContentTypeDefinition ContentTypeDefinition { get; set; }
}
