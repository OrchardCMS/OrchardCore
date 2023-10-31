using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentPartImportHandler
{
    IReadOnlyCollection<ImportColumn> Columns(ImportContentPartContext context);

    Task MapAsync(ContentPartImportMapContext content);

    Task MapOutAsync(ContentPartExportMapContext content);

    Task ValidateColumnsAsync(ValidatePartImportContext context);
}
