using YesSql.Filters.Query;

namespace OrchardCore.ContentTransfer;

public interface IContentTransferEntryAdminListFilterProvider
{
    void Build(QueryEngineBuilder<ContentTransferEntry> builder);
}
