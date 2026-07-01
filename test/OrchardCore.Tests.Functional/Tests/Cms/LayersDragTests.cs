using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based widget/zone drag-and-drop on the Layers admin
// page (src/OrchardCore.Modules/OrchardCore.Layers/Views/Admin/Index.cshtml),
// which persists via one fetch() POST per drop (rather than a form submit)
// and shows an undo banner - the Blog recipe ships a single "Footer" widget
// (RawHtml) in the "Always" layer's "Footer" zone, with "Content" as the
// only other zone, so the test moves it there and back.
public sealed class LayersDragTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public LayersDragTests(BlogFixture fixture) : base(fixture) { }

    private static ILocator WidgetItem(IPage page, string zoneName, string widgetText)
        => page.Locator($".layer-zone[data-zone='{zoneName}'] .list-group-item").Filter(new LocatorFilterOptions { HasText = widgetText });

    [Fact]
    public async Task LayersDrag_MoveWidgetToDifferentZone_PersistsAndUndoRestores()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/Layers");

        var footerWidget = WidgetItem(page, "Footer", "Footer");
        await footerWidget.WaitForAsync();

        var contentZoneList = page.Locator(".layer-zone[data-zone='Content'] ul.list-group");

        await page.DragAsync(footerWidget.Locator(".properties"), contentZoneList);

        // A successful move triggers a fetch() POST and shows the undo banner.
        await page.Locator("#layer-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await WidgetItem(page, "Content", "Footer").WaitForAsync();
        Assert.Equal(0, await WidgetItem(page, "Footer", "Footer").CountAsync());

        // Restore the original zone via the undo banner's link so re-running
        // this test (or others depending on the recipe's original layout)
        // isn't affected by a leftover move from a prior run.
        await page.GotoAndAssertOkAsync("/Admin/Layers");
        await page.DragAsync(
            WidgetItem(page, "Content", "Footer").Locator(".properties"),
            page.Locator(".layer-zone[data-zone='Footer'] ul.list-group"));
        await page.Locator("#layer-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WidgetItem(page, "Footer", "Footer").WaitForAsync();
        Assert.Equal(0, await WidgetItem(page, "Content", "Footer").CountAsync());

        await page.CloseAsync();
    }
}
