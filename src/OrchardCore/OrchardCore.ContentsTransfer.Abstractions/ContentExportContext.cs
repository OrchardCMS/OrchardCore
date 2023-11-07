using System.Data;

namespace OrchardCore.ContentsTransfer;

public class ContentExportContext : ImportContentContext
{
    public DataRow Row { get; set; }
}
