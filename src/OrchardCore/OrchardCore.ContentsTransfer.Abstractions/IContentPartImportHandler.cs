using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentPartImportHandler
{
    IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context);

    Task ImportAsync(ContentPartImportMapContext content);

    Task ExportAsync(ContentPartExportMapContext content);
}
