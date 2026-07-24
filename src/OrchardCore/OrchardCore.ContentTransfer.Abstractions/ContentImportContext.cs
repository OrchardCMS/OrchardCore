using System.Data;

namespace OrchardCore.ContentTransfer;

public sealed class ContentImportContext : ImportContentContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
