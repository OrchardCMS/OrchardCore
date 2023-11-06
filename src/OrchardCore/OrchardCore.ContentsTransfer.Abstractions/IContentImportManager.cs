using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportManager
{
    Task<IReadOnlyCollection<ImportColumn>> GetColumnsAsync(ImportContentContext context);

    Task ImportAsync(ContentImportMapContext context);

    Task ExportAsync(ContentExportMapContext context);

    Task<ContentValidateResult> ValidateAsync(ValidateImportContext context);
}
