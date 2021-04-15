using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.ContentManagement
{
    public static class ContentQueryExtensions
    {
        public static async Task<ContentItem> FirstOrDefaultAsync(this IQuery<ContentItem> query, IContentManager contentManager)
            => await contentManager.LoadAsync(await query.FirstOrDefaultAsync());

        public static async Task<IEnumerable<ContentItem>> ListAsync(this IQuery<ContentItem> query, IContentManager contentManager)
            => await contentManager.LoadAsync(await query.ListAsync());

        public static IAsyncEnumerable<ContentItem> ToAsyncEnumerable(this IQuery<ContentItem> query, IContentManager contentManager)
            => contentManager.LoadAsync(query.ToAsyncEnumerable());
    }
}
