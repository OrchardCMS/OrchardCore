using OrchardCore.ContentTransfer.Models;
using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.ContentTransfer.Services;

public sealed class DefaultContentTransferEntryAdminListQueryService : IContentTransferEntryAdminListQueryService
{
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;

    public DefaultContentTransferEntryAdminListQueryService(
        ISession session,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _serviceProvider = serviceProvider;
    }

    public async Task<ContentTransferEntryQueryResult> QueryAsync(int page, int pageSize, ListContentTransferEntryOptions options, IUpdateModel updater)
    {
        var indexedQuery = _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.Direction == options.Direction);

        if (!string.IsNullOrWhiteSpace(options.Owner))
        {
            indexedQuery = indexedQuery.Where(x => x.Owner == options.Owner);
        }

        IQuery<ContentTransferEntry> query = indexedQuery;

        query = await options.FilterResult.ExecuteAsync(new ContentTransferEntryQueryContext(_serviceProvider, query));

        // Query the count before applying pagination logic.
        var totalCount = await query.CountAsync();

        if (pageSize > 0)
        {
            if (page > 1)
            {
                query = query.Skip((page - 1) * pageSize);
            }

            query = query.Take(pageSize);
        }

        return new ContentTransferEntryQueryResult()
        {
            Entries = await query.ListAsync(),
            TotalCount = totalCount,
        };
    }
}
