using System.Data;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentTransfer;

public sealed class ContentFieldExportMapContext : ImportContentFieldContext
{
    public ContentField ContentField { get; set; }

    public DataRow Row { get; set; }
}
