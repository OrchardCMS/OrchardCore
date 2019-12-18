using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class DataLoaderExtensions
    {
        public static IDataLoader<string, ContentItem> GetorAddPublishedContentItemByIdDataLoader<T>(this ResolveFieldContext<T> context)
        {
            var serviceProvider = context.ResolveServiceProvider();

            var accessor = serviceProvider.GetRequiredService<IDataLoaderContextAccessor>();
            var session = serviceProvider.GetService<ISession>();

            return accessor.Context.GetOrAddBatchLoader<string, ContentItem>("GetPublishedContentItemsById", ci => LoadPublishedContentItems(ci, session));
        }

        private static async Task<IDictionary<string, ContentItem>> LoadPublishedContentItems(IEnumerable<string> contentItemIds, ISession session)
        {
            if (contentItemIds is null || !contentItemIds.Any())
            {
                return new Dictionary<string, ContentItem>();
            }

            var contentItemsLoaded = await session.Query<ContentItem, ContentItemIndex>(y => y.ContentItemId.IsIn(contentItemIds) && y.Published).ListAsync();
            return contentItemsLoaded.ToDictionary(k => k.ContentItemId, v => v);
        }
    }
}
