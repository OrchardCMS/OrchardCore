using System.Data;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer;

public class ContentFieldExportMapContext : ImportContentFieldContext
{
    public ContentField ContentField { get; set; }

    public DataRow Row { get; set; }
}
