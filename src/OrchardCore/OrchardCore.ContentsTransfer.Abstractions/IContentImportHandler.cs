using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportHandler
{
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context);

    Task ImportAsync(ContentImportMapContext content);

    Task ExportAsync(ContentExportMapContext content);

    Task ValidateAsync(ValidateImportContext context);
}
