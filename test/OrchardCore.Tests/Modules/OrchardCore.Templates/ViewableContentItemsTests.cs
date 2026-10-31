using OrchardCore.ContentManagement;
using OrchardCore.Templates.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public class ViewableContentItemsTests
{
    [Fact]
    public async Task TakeAsync_RestrictedItems_SkipsThemAndLoadsNextPages()
    {
        // Arrange
        var contentItems = CreateContentItems(20);
        var loadedPages = 0;

        // Act: every other content item can't be viewed, so the first page doesn't have enough of them.
        var viewable = await ViewableContentItems.TakeAsync(
            (page, pageSize) =>
            {
                loadedPages++;
                return Task.FromResult(contentItems.Skip(page * pageSize).Take(pageSize));
            },
            contentItem => Task.FromResult(int.Parse(contentItem.ContentItemId) % 2 == 0),
            count: 5,
            maxPages: 10);

        // Assert
        Assert.Equal(["0", "2", "4", "6", "8"], viewable.Select(contentItem => contentItem.ContentItemId));
        Assert.Equal(2, loadedPages);
    }

    [Fact]
    public async Task TakeAsync_LastPageShort_StopsLoading()
    {
        // Arrange
        var contentItems = CreateContentItems(7);
        var loadedPages = 0;

        // Act
        var viewable = await ViewableContentItems.TakeAsync(
            (page, pageSize) =>
            {
                loadedPages++;
                return Task.FromResult(contentItems.Skip(page * pageSize).Take(pageSize));
            },
            contentItem => Task.FromResult(contentItem.ContentItemId != "3"),
            count: 5,
            maxPages: 10);

        // Assert
        Assert.Equal(["0", "1", "2", "4", "5"], viewable.Select(contentItem => contentItem.ContentItemId));
        Assert.Equal(2, loadedPages);
    }

    [Fact]
    public async Task TakeAsync_NothingViewable_StopsAtMaxPages()
    {
        // Arrange
        var contentItems = CreateContentItems(100);
        var loadedPages = 0;

        // Act
        var viewable = await ViewableContentItems.TakeAsync(
            (page, pageSize) =>
            {
                loadedPages++;
                return Task.FromResult(contentItems.Skip(page * pageSize).Take(pageSize));
            },
            contentItem => Task.FromResult(false),
            count: 5,
            maxPages: 3);

        // Assert
        Assert.Empty(viewable);
        Assert.Equal(3, loadedPages);
    }

    private static List<ContentItem> CreateContentItems(int count)
        => Enumerable.Range(0, count).Select(index => new ContentItem { ContentItemId = index.ToString(CultureInfo.InvariantCulture) }).ToList();
}
