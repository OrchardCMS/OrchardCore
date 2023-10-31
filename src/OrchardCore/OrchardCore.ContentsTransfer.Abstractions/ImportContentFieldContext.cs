using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer;

public class ImportContentFieldContext
{
    public ContentItem ContentItem { get; set; }

    public ContentField ContentField { get; set; }

    public ContentPartFieldDefinition ContentPartFieldDefinition { get; set; }

    public string PartName { get; set; }

    public ContentPart ContentPart { get; set; }
}
