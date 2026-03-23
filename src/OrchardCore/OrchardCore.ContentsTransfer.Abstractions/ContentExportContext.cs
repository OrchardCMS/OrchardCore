using System.Data;

namespace OrchardCore.ContentsTransfer;

public sealed class ContentExportContext : ImportContentContext
{
    public DataRow Row { get; set; }
}
