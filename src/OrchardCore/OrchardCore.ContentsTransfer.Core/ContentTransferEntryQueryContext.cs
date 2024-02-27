using System;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.ContentsTransfer;

public class ContentTransferEntryQueryContext : QueryExecutionContext<ContentTransferEntry>
{
    public ContentTransferEntryQueryContext(IServiceProvider serviceProvider, IQuery<ContentTransferEntry> query)
        : base(query)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }
}
