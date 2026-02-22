using System.Data;

namespace OrchardCore.ContentsTransfer;

public class ContentFieldImportMapContext : ImportContentFieldContext
{
    public DataColumnCollection Columns { get; set; }

    public DataRow Row { get; set; }
}
