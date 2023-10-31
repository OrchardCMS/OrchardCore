using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportHandlerCoordinator
{
    IReadOnlyCollection<ImportColumn> Columns(ImportContentContext context);

    Task MapAsync(ContentImportMapContext context);

    Task MapOutAsync(ContentExportMapContext context);

    Task ValidateColumnsAsync(ValidateImportContext context);
}
