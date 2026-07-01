using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based drag-reorder on the admin Dashboard Manage
// page (src/OrchardCore.Modules/OrchardCore.AdminDashboard/Views/Dashboard/
// Manage.cshtml), persisted via one fetch() POST per drop (like Layers). No
// shipped recipe configures any dashboard widgets, so this uses the custom
// WidgetDragTests recipe/fixture, which seeds two HtmlDashboardWidget items
// ("Dashboard Widget One" at position 0, "Dashboard Widget Two" at position 1).
public sealed class AdminDashboardDragTests : CmsTestBase<WidgetDragTestsFixture>, IClassFixture<WidgetDragTestsFixture>
{
    public AdminDashboardDragTests(WidgetDragTestsFixture fixture) : base(fixture) { }

    private static ILocator Widget(IPage page, string titleText)
        => page.Locator(".dashboard-wrapper").Filter(new LocatorFilterOptions { HasText = titleText });

    [Fact]
    public async Task AdminDashboardDrag_ReorderTwoWidgets_PersistsOnReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/dashboard/manage");

        var container = page.Locator("#container");
        await container.WaitForAsync();

        var widgetOne = Widget(page, "Dashboard Widget One");
        var widgetTwo = Widget(page, "Dashboard Widget Two");
        await widgetOne.WaitForAsync();
        await widgetTwo.WaitForAsync();

        // Widget One starts before Widget Two (position 0 vs 1).
        var initialOrder = await container.Locator(".dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains("Dashboard Widget One", initialOrder[0]);
        Assert.Contains("Dashboard Widget Two", initialOrder[1]);

        await page.DragAsync(widgetOne.Locator(".dashboard-handle"), widgetTwo);

        // A successful move triggers a fetch() POST and shows the undo banner.
        await page.Locator("#dashboard-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var reorderedOrder = await container.Locator(".dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains("Dashboard Widget Two", reorderedOrder[0]);
        Assert.Contains("Dashboard Widget One", reorderedOrder[1]);

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var persistedOrder = await page.Locator("#container .dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains("Dashboard Widget Two", persistedOrder[0]);
        Assert.Contains("Dashboard Widget One", persistedOrder[1]);

        // Restore the original order so re-running this test isn't affected
        // by a leftover reorder from a prior run.
        await page.DragAsync(Widget(page, "Dashboard Widget Two").Locator(".dashboard-handle"), Widget(page, "Dashboard Widget One"));
        await page.Locator("#dashboard-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var restoredOrder = await page.Locator("#container .dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains("Dashboard Widget One", restoredOrder[0]);
        Assert.Contains("Dashboard Widget Two", restoredOrder[1]);

        await page.CloseAsync();
    }
}
