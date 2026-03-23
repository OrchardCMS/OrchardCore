using System.Data;

namespace OrchardCore.ContentsTransfer;

public sealed class ContentImportContext : ImportContentContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
