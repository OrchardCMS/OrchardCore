using OrchardCore.ContentManagement;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Takes the content items a user can view from pages of content items, e.g. the most recent ones, so that a list
/// of content items only shows those the user is allowed to see.
/// </summary>
public static class ViewableContentItems
{
    /// <summary>
    /// Takes the first content items the user can view, loading the pages one after the other until there are enough
    /// of them, or no more content items, or the maximum number of pages was loaded.
    /// </summary>
    /// <param name="loadPageAsync">Loads a page of content items from its index and size.</param>
    /// <param name="canViewAsync">Whether the user can view a content item.</param>
    /// <param name="count">The number of content items to take.</param>
    /// <param name="maxPages">The maximum number of pages to load, which bounds the content items checked.</param>
    /// <returns>The content items the user can view, in the order of the pages.</returns>
    public static async Task<IReadOnlyList<ContentItem>> TakeAsync(
        Func<int, int, Task<IEnumerable<ContentItem>>> loadPageAsync,
        Func<ContentItem, Task<bool>> canViewAsync,
        int count,
        int maxPages)
    {
        List<ContentItem> contentItems = [];

        for (var page = 0; page < maxPages && contentItems.Count < count; page++)
        {
            var pageItems = (await loadPageAsync(page, count)).ToList();

            foreach (var contentItem in pageItems)
            {
                if (contentItems.Count < count && await canViewAsync(contentItem))
                {
                    contentItems.Add(contentItem);
                }
            }

            if (pageItems.Count < count)
            {
                break;
            }
        }

        return contentItems;
    }
}
