using System.Data;

namespace OrchardCore.ContentTransfer;

public sealed class ContentExportContext : ImportContentContext
{
    public DataRow Row { get; set; }
}
