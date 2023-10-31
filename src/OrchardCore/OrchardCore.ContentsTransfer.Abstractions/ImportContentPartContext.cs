using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer;

public class ImportContentPartContext
{
    public ContentPart ContentPart { get; set; }

    public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
}
