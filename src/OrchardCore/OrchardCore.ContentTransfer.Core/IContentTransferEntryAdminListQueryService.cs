using OrchardCore.ContentTransfer.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTransfer;

public interface IContentTransferEntryAdminListQueryService
{
    Task<ContentTransferEntryQueryResult> QueryAsync(int page, int pageSize, ListContentTransferEntryOptions options, IUpdateModel updater);
}
