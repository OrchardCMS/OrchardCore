using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Services;

public interface IContainerService
{
    /// <summary>
    /// Gets the total count of content items associated with a specified list content item ID, filtered according
    /// to the provided options.
    /// </summary>
    Task<int> GetItemCountAsync(string listContentItemId, ContainedItemOptions options);

    /// <summary>
    /// Queries contained items by page, either ordered by the created UTC timestamp or by a specified order value.
    /// </summary>
    /// <param name="contentItemId">The ID of the content item containing the contained items.</param>
    /// <param name="enableOrdering">A value indicating whether to enable ordering.</param>
    /// <param name="pager">The <see cref="Pager"/> for controlling pagination.</param>
    /// <param name="containedItemOptions">the <see cref="ContainedItemOptions"/> to filter the results</param>
    /// <returns>The list of contained <see cref="ContentItem"/>s.</returns>
    Task<IEnumerable<ContentItem>> QueryContainedItemsAsync(string contentItemId, bool enableOrdering, Pager pager, ContainedItemOptions containedItemOptions);

    /// <summary>
    /// Queries contained items by page, either ordered by the created UTC timestamp or by a specified order value.
    /// </summary>
    /// <param name="contentItemId">The ID of the content item containing the contained items.</param>
    /// <param name="enableOrdering">A value indicating whether to enable ordering.</param>
    /// <param name="pager">The <see cref="PagerSlim"/> for controlling pagination.</param>
    /// <param name="containedItemOptions">the <see cref="ContainedItemOptions"/> to filter the results</param>
    /// <returns>The list of contained <see cref="ContentItem"/>s.</returns>
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
