using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportManager
{
    Task<IReadOnlyCollection<ImportColumn>> GetColumnsAsync(ImportContentContext context);

    Task ImportAsync(ContentImportMapContext context);

    Task ExportAsync(ContentExportMapContext context);

    Task ValidateAsync(ValidateImportContext context);
}
