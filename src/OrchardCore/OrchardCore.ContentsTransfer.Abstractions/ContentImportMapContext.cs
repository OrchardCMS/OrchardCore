using System.Data;

namespace OrchardCore.ContentsTransfer;

public class ContentImportMapContext : ImportContentContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
