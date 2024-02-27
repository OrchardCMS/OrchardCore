using System.Threading.Tasks;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentsTransfer;

public interface IContentTransferEntryAdminListQueryService
{
    Task<ContentTransferEntryQueryResult> QueryAsync(int page, int pageSize, ListContentTransferEntryOptions options, IUpdateModel updater);
}
