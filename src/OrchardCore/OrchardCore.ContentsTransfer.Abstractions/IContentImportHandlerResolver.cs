using System.Collections.Generic;

namespace OrchardCore.ContentsTransfer;

public interface IContentImportHandlerResolver
{
    IList<IContentFieldImportHandler> GetFieldHandlers(string fieldName);

    IList<IContentPartImportHandler> GetPartHandlers(string partName);
}
