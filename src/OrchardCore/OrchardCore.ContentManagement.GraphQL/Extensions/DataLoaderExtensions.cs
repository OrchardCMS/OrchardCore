using GraphQL;
using GraphQL.DataLoader;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentManagement.GraphQL;

public static class DataLoaderExtensions
{
    public static IDataLoader<string, IEnumerable<ContentItem>> GetOrAddContentItemByIdDataLoader<T>(this IResolveFieldContext<T> context)
    {
        var accessor = context.RequestServices.GetRequiredService<IDataLoaderContextAccessor>();
        var session = context.RequestServices.GetService<ISession>();
        var status = context.GetArgument<PublicationStatusEnum>("status");

        return accessor.Context.GetOrAddCollectionBatchLoader<string, ContentItem>($"GetContentItemsById_{status}", ci => LoadContentItemsAsync(ci, session, status));
    }

    public static async Task<ILookup<string, ContentItem>> LoadContentItemsAsync(IEnumerable<string> contentItemIds, ISession session, PublicationStatusEnum status)
    {
        if (contentItemIds is null || !contentItemIds.Any())
        {
            return default;
        }

        var query = session.Query<ContentItem, ContentItemIndex>(y => y.ContentItemId.IsIn(contentItemIds));
        query = FilterVersion(query, status);

        var contentItemsLoaded = await query.ListAsync();
        return contentItemsLoaded.ToLookup(k => k.ContentItemId, v => v);
    }

    private static IQuery<ContentItem, ContentItemIndex> FilterVersion(IQuery<ContentItem, ContentItemIndex> query, PublicationStatusEnum status)
    {
        if (status == PublicationStatusEnum.Published)
        {
            query = query.Where(q => q.Published);
        }
        else if (status == PublicationStatusEnum.Draft)
        {
            query = query.Where(q => q.Latest && !q.Published);
        }
        else if (status == PublicationStatusEnum.Latest)
        {
            query = query.Where(q => q.Latest);
        }

        return query;
    }
}
