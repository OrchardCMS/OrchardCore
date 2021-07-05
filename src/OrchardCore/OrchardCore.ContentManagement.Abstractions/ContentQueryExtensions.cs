using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Allows to execute an <see cref="IQuery{ContentItem}"/> to return matching <see cref="ContentItem"/>s
    /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
    /// </summary>
    public static class ContentQueryExtensions
    {
        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem}"/> to return the first matching <see cref="ContentItem"/>
        /// but after having invoked the load events on it by using the <see cref="IContentManager"/>.
        /// </summary>
        public static async Task<ContentItem> FirstOrDefaultAsync(this IQuery<ContentItem> query, IContentManager contentManager)
        {
            var contentItem = await query.FirstOrDefaultAsync();
            
            if (contentItem == null)
            {
                return null;
            }
            
            return await contentManager.LoadAsync(contentItem);
        }

        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem}"/> to return all matching <see cref="ContentItem"/>s
        /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
        /// </summary>
        public static async Task<IEnumerable<ContentItem>> ListAsync(this IQuery<ContentItem> query, IContentManager contentManager)
            => await contentManager.LoadAsync(await query.ListAsync());

        /// <summary>
        /// Executes this <see cref="IQuery{ContentItem}"/> to return all matching <see cref="ContentItem"/>s
        /// but after having invoked the load events on them by using the <see cref="IContentManager"/>.
        /// </summary>
        public static IAsyncEnumerable<ContentItem> ToAsyncEnumerable(this IQuery<ContentItem> query, IContentManager contentManager)
            => contentManager.LoadAsync(query.ToAsyncEnumerable());
    }
}
