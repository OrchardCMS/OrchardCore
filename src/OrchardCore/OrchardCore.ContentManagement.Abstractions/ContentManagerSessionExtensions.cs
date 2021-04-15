using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement
{
    public static class ContentManagerSessionExtensions
    {
        public static async Task<ContentItem> FirstOrDefaultAsync(this IQuery<ContentItem> query, IContentManager contentManager)
            => await contentManager.LoadAsync(await query.FirstOrDefaultAsync());

        public static async Task<ContentItem> FirstOrDefaultAsync<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
            => await contentManager.LoadAsync(await query.FirstOrDefaultAsync());

        public static async Task<IEnumerable<ContentItem>> ListAsync(this IQuery<ContentItem> query, IContentManager contentManager)
            => await contentManager.LoadAsync(await query.ListAsync());

        public static async Task<IEnumerable<ContentItem>> ListAsync<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
            => await contentManager.LoadAsync(await query.ListAsync());

        public static IAsyncEnumerable<ContentItem> ToAsyncEnumerable(this IQuery<ContentItem> query, IContentManager contentManager)
            => contentManager.LoadAsync(query.ToAsyncEnumerable());

        public static IAsyncEnumerable<ContentItem> ToAsyncEnumerable<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
            => contentManager.LoadAsync(query.ToAsyncEnumerable());
    }
}
