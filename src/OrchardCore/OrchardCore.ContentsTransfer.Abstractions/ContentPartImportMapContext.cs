using System.Data;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;

public class ContentPartImportMapContext : ImportContentPartContext
{
    public ContentItem ContentItem { get; set; }

    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
