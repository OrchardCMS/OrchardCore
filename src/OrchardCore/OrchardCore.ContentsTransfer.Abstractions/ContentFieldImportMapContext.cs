using System.Data;

namespace OrchardCore.ContentsTransfer;

public sealed class ContentFieldImportMapContext : ImportContentFieldContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
