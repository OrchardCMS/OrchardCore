using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Services
{
    public interface IContainerService
    {
        /// <summary>
        /// Query contained items by page either order by the created utc or order value.
        /// </summary>
        Task<IEnumerable<ContentItem>> QueryContainedItemsAsync(string contentItemId, bool enableOrdering, PagerSlim pager, ContainedItemOptions containedItemOptions);

        /// <summary>
        /// Update the orders of the content items.
        /// </summary>
        Task UpdateContentItemOrdersAsync(IEnumerable<ContentItem> contentItems, int orderOfFirstItem);

        /// <summary>
        /// Get the next order number.
        /// New or cloned content items are added to the bottom of the list.
        /// </summary>
        Task<int> GetNextOrderNumberAsync(string contentItemId);

        /// <summary>
        /// Update order of the content items when ordering is enabled.
        /// </summary>
        Task SetInitialOrder(string containerContentType);
    }
}
