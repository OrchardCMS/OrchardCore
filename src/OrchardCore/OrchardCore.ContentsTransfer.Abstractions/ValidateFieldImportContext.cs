using System.Data;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentsTransfer;

public class ValidateFieldImportContext : ImportContentFieldContext
{
    public DataColumnCollection Columns { get; set; }

    public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
}
