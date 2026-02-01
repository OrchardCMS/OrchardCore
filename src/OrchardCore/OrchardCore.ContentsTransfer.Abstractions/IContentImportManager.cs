using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportManager
{
    Task<IReadOnlyCollection<ImportColumn>> GetColumnsAsync(ImportContentContext context);

    /// <summary>
    /// Maps the DataRow to ContentItem.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task ImportAsync(ContentImportContext context);

    /// <summary>
    /// Maps the ContentItem to a DataRow.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task ExportAsync(ContentExportContext context);

    // Task<ContentValidateResult> ValidateAsync(ValidateImportContext context);
}
