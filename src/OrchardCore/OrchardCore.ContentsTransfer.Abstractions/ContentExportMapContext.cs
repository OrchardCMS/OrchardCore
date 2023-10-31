using System.Data;

namespace OrchardCore.ContentsTransfer;

public class ContentExportMapContext : ImportContentContext
{
    public DataRow Row { get; set; }
}
