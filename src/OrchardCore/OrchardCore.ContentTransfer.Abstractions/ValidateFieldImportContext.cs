using System.Data;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentTransfer;

public sealed class ValidateFieldImportContext : ImportContentFieldContext
{
    public DataColumnCollection Columns { get; set; }

    public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
}
