using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Allows to execute an <see cref="IQuery{ContentItem, TIndex}"/> to return matching <see cref="ContentItem"/>s
    /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
    /// </summary>
    public static class ContentQueryOfTIndexExtensions
    {
        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem}"/> to return the first matching <see cref="ContentItem"/>
        /// but after having invoked the load events on it by using the <see cref="IContentManager"/>.
        /// </summary>
        public static async Task<ContentItem> FirstOrDefaultAsync<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
        {
            var contentItem = await query.FirstOrDefaultAsync();

            if (contentItem == null)
            {
                return null;
            }

            return await contentManager.LoadAsync(contentItem);
        }

        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem, TIndex}"/> to return all matching <see cref="ContentItem"/>s
        /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
        /// </summary>
        public static async Task<IEnumerable<ContentItem>> ListAsync<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
            => await contentManager.LoadAsync(await query.ListAsync());

        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem, TIndex}"/> to return all matching <see cref="ContentItem"/>s
        /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
        /// </summary>
        public static IAsyncEnumerable<ContentItem> ToAsyncEnumerable<TIndex>(this IQuery<ContentItem, TIndex> query, IContentManager contentManager)
            where TIndex : class, IIndex
            => contentManager.LoadAsync(query.ToAsyncEnumerable());
    }
}
