using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer;

public interface IContentFieldImportHandler
{
    IReadOnlyCollection<ImportColumn> Columns(ImportContentFieldContext context);

    Task MapAsync(ContentFieldImportMapContext context);

    Task MapOutAsync(ContentFieldExportMapContext context);

    Task ValidateColumnsAsync(ValidateFieldImportContext context);
}
