using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportHandler
{
    IReadOnlyCollection<ImportColumn> Columns(ImportContentContext context);

    Task MapAsync(ContentImportMapContext content);

    Task MapOutAsync(ContentExportMapContext content);

    Task ValidateColumnsAsync(ValidateImportContext context);
}
