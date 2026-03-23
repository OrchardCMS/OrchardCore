using System.Data;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentTransfer;

public sealed class ValidateImportContext : ImportContentContext
{
    public DataColumnCollection Columns { get; set; }

    public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
}
