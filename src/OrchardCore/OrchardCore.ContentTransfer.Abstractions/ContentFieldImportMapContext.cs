using System.Data;

namespace OrchardCore.ContentTransfer;

public sealed class ContentFieldImportMapContext : ImportContentFieldContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
