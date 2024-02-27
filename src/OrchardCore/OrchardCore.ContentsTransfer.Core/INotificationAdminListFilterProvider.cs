using YesSql.Filters.Query;

namespace OrchardCore.ContentsTransfer;

public interface IContentTransferEntryAdminListFilterProvider
{
    void Build(QueryEngineBuilder<ContentTransferEntry> builder);
}
