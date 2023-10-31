using System.Data;

namespace OrchardCore.ContentsTransfer;

public class ContentFieldExportMapContext : ImportContentFieldContext
{
    public DataRow Row { get; set; }
}
