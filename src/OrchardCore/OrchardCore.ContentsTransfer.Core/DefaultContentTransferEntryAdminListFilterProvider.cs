using System;
using System.Threading.Tasks;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Models;
using YesSql;
using YesSql.Filters.Query;

namespace OrchardCore.ContentsTransfer;

public class DefaultContentTransferEntryAdminListFilterProvider : IContentTransferEntryAdminListFilterProvider
{
    public void Build(QueryEngineBuilder<ContentTransferEntry> builder)
    {
        builder
            .WithNamedTerm("status", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    if (Enum.TryParse<ContentTransferEntryStatus>(val, true, out var status))
                    {
                        return new ValueTask<IQuery<ContentTransferEntry>>(query.With<ContentTransferEntryIndex>(x => x.Status == status));
                    }

                    return new ValueTask<IQuery<ContentTransferEntry>>(query);
                })
                .MapTo<ListContentTransferEntryOptions>((val, model) =>
                {
                    if (Enum.TryParse<ContentTransferEntryStatus>(val, true, out var status))
                    {
                        model.Status = status;
                    }
                })
                .MapFrom<ListContentTransferEntryOptions>((model) =>
                {
                    if (model.Status.HasValue)
                    {
                        return (true, model.Status.ToString());
                    }

                    return (false, string.Empty);
                })
                .AlwaysRun()
             )
            .WithNamedTerm("sort", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    if (Enum.TryParse<ContentTransferEntryOrder>(val, true, out var sort) && sort == ContentTransferEntryOrder.Oldest)
                    {
                        return new ValueTask<IQuery<ContentTransferEntry>>(query.With<ContentTransferEntryIndex>().OrderBy(x => x.CreatedUtc));
                    }

                    return new ValueTask<IQuery<ContentTransferEntry>>(query.With<ContentTransferEntryIndex>().OrderByDescending(x => x.CreatedUtc));
                })
                .MapTo<ListContentTransferEntryOptions>((val, model) =>
                {
                    if (Enum.TryParse<ContentTransferEntryOrder>(val, true, out var sort))
                    {
                        model.OrderBy = sort;
                    }
                })
                .MapFrom<ListContentTransferEntryOptions>((model) =>
                {
                    if (model.OrderBy.HasValue)
                    {
                        return (true, model.OrderBy.ToString());
                    }

                    return (false, string.Empty);
                })
                .AlwaysRun()
            );
    }
}
