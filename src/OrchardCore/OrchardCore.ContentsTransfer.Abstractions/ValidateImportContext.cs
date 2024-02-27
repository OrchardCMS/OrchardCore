using System.Data;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentsTransfer;

public class ValidateImportContext : ImportContentContext
{
    public DataColumnCollection Columns { get; set; }

    public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
}
