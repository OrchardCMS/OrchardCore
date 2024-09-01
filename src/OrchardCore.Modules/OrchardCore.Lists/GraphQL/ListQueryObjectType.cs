using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lists.GraphQL;

public class ListQueryObjectType : ObjectGraphType<ListPart>
{
    public ListQueryObjectType(IStringLocalizer<ListQueryObjectType> S)
    {
        Name = "ListPart";
        Description = S["Represents a collection of content items."];

        Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>("contentItems")
            .Description("the content items")
            .Argument<IntGraphType>("first", "the first n elements (10 by default)", config => config.DefaultValue = 10)
            .Argument<IntGraphType>("skip", "the number of elements to skip", config => config.DefaultValue = 0)
            // Important to use ResolveLockedAsync to prevent concurrency error on database query, when using nested content items with List part
            .ResolveLockedAsync(async g =>
            {
                var serviceProvider = g.RequestServices;
                var session = serviceProvider.GetService<ISession>();
                var accessor = serviceProvider.GetRequiredService<IDataLoaderContextAccessor>();

                var dataLoader = accessor.Context.GetOrAddCollectionBatchLoader<string, ContentItem>("ContainedPublishedContentItems",
                   x => LoadPublishedContentItemsForListAsync(x, session, g.GetArgument<int>("skip"), g.GetArgument<int>("first")));

                return await dataLoader.LoadAsync(g.Source.ContentItem.ContentItemId).GetResultAsync();
            });
    }

    private static async Task<ILookup<string, ContentItem>> LoadPublishedContentItemsForListAsync(IEnumerable<string> contentItemIds, ISession session, int skip, int count)
    {
        if (contentItemIds is null || !contentItemIds.Any())
        {
            return new Dictionary<string, ContentItem>().ToLookup(k => k.Key, v => v.Value);
        }

        var query = await session.Query<ContentItem>()
                                 .With<ContentItemIndex>(ci => ci.Published)
                                 .With<ContainedPartIndex>(cp => cp.ListContentItemId.IsIn(contentItemIds))
                                 .OrderBy(o => o.Order)
                                 .Skip(skip)
                                 .Take(count)
                                 .ListAsync();

        return query.ToLookup(k => k.As<ContainedPart>().ListContentItemId);
    }
}
