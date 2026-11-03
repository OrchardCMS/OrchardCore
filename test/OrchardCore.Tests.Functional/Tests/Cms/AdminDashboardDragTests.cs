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

    // Covers the pointer-based (not SortableJS) snap-to-grid resize handles added in
    // dashboard.ts's beginResize()/snapTo(): dragging .dashboard-resize-handle-se resizes
    // by whole grid cells (cellSize.width/height, recalculated from the container's actual
    // computed grid-template-columns/-rows on load/resize - not a hardcoded constant), then
    // persists via the same fetch() POST + undo-banner mechanism as reordering.
    [Fact]
    public async Task ResizeWidget_DragHandle_PersistsGridDimensions()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/dashboard/manage");

        var container = page.Locator("#container");
        await container.WaitForAsync();

        var widgetOne = Widget(page, "Dashboard Widget One");
        await widgetOne.WaitForAsync();

        // Both start at their recipe-seeded default 1x1 grid size.
        var initialWidth = await widgetOne.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--dashboard-width') || '1'");
        var initialHeight = await widgetOne.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--dashboard-height') || '1'");
        Assert.Equal("1", initialWidth.Trim());
        Assert.Equal("1", initialHeight.Trim());

        // Real actual per-cell pixel size (parsed from the live grid-template-columns/-rows
        // computed style, exactly like the script's own calculateCellSize()), so the drag
        // distance below reliably snaps to +1 column and +1 row rather than guessing a
        // fixed pixel delta against an assumed cell size.
        var cellSize = await container.EvaluateAsync<CellSizeResult>(
            """
            (el) => {
                const styles = getComputedStyle(el);
                const columns = styles.getPropertyValue('grid-template-columns').split(' ');
                const rows = styles.getPropertyValue('grid-template-rows').split(' ');
                return {
                    width: parseFloat(columns[0]),
                    height: parseFloat(rows[0]),
                    gapWidth: parseFloat(styles.getPropertyValue('grid-column-gap')) || 0,
                    gapHeight: parseFloat(styles.getPropertyValue('grid-row-gap')) || 0,
                };
            }
            """);

        var handle = widgetOne.Locator(".dashboard-resize-handle-se");
        await handle.WaitForAsync();
        var handleBox = await handle.BoundingBoxAsync();
        Assert.NotNull(handleBox);

        var startX = handleBox!.X + (handleBox.Width / 2);
        var startY = handleBox.Y + (handleBox.Height / 2);

        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();
        // snapTo() computes updated.width = original.width (1 cell, i.e. cellSize.width
        // pixels) + raw pointer delta, then ceil(updated/cellSize.width) when growing - so
        // to land on exactly +1 column/row (snap value 2) the raw delta only needs to push
        // updated.width into (cellSize.width, 2*cellSize.width], not a full extra cell+gap
        // (that would overshoot into the 3rd cell). Half a cell comfortably clears the
        // "exactly 1x" floor while staying under the "over 2x" ceiling.
        var deltaX = cellSize.Width / 2;
        var deltaY = cellSize.Height / 2;
        await page.Mouse.MoveAsync((float)(startX + deltaX), (float)(startY + deltaY), new MouseMoveOptions { Steps = 5 });
        await page.Mouse.UpAsync();

        // A successful resize triggers the same fetch() POST + undo banner as reordering.
        await page.Locator("#dashboard-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var resizedWidth = await widgetOne.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-width')");
        var resizedHeight = await widgetOne.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-height')");
        Assert.Equal("2", resizedWidth.Trim());
        Assert.Equal("2", resizedHeight.Trim());

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedWidget = Widget(page, "Dashboard Widget One");
        var persistedWidth = await reloadedWidget.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-width')");
        var persistedHeight = await reloadedWidget.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-height')");
        Assert.Equal("2", persistedWidth.Trim());
        Assert.Equal("2", persistedHeight.Trim());

        // Restore the original 1x1 size so re-running this test isn't affected by a
        // leftover resize from a prior run.
        var restoreHandle = reloadedWidget.Locator(".dashboard-resize-handle-se");
        var restoreBox = await restoreHandle.BoundingBoxAsync();
        Assert.NotNull(restoreBox);

        await page.Mouse.MoveAsync(restoreBox!.X + (restoreBox.Width / 2), restoreBox.Y + (restoreBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(
            (float)(restoreBox.X + (restoreBox.Width / 2) - deltaX),
            (float)(restoreBox.Y + (restoreBox.Height / 2) - deltaY),
            new MouseMoveOptions { Steps = 5 });
        await page.Mouse.UpAsync();
        await page.Locator("#dashboard-undo-message").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var restoredWidget = Widget(page, "Dashboard Widget One");
        var restoredWidth = await restoredWidget.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-width') || '1'");
        var restoredHeight = await restoredWidget.EvaluateAsync<string>("el => el.style.getPropertyValue('--dashboard-height') || '1'");
        Assert.Equal("1", restoredWidth.Trim());
        Assert.Equal("1", restoredHeight.Trim());

        await page.CloseAsync();
    }

    private sealed class CellSizeResult
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double GapWidth { get; set; }
        public double GapHeight { get; set; }
    }
}
