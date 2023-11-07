using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportHandler
{
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context);

    Task ImportAsync(ContentImportContext content);

    Task ExportAsync(ContentExportContext content);

    Task ValidateAsync(ValidateImportContext context);
}
