using System.Data;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;

public sealed class ContentPartExportMapContext : ImportContentPartContext
{
    public ContentPart ContentPart { get; set; }

    public ContentItem ContentItem { get; set; }

    public DataRow Row { get; set; }
}
