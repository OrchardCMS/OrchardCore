using System.Data;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;

public class ContentPartExportMapContext : ImportContentPartContext
{
    public ContentItem ContentItem { get; set; }

    public DataRow Row { get; set; }
}
