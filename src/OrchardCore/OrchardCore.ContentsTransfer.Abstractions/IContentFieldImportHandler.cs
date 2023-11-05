using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentFieldImportHandler
{
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context);

    Task ImportAsync(ContentFieldImportMapContext context);

    Task ExportAsync(ContentFieldExportMapContext context);

    Task ValidateAsync(ValidateFieldImportContext context);
}
