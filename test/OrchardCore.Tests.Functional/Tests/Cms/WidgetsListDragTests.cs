using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based cross-zone widget drag-and-drop on
// WidgetsListPart editors (src/OrchardCore.Modules/OrchardCore.Widgets/Views/
// WidgetsListPart.Edit.cshtml) - no shipped recipe configures a
// WidgetsListPart, so this uses the custom WidgetDragTests recipe/fixture
// (test/OrchardCore.Tests.Functional/Fixtures/widget-drag-tests.recipe.json),
// which seeds a "Widgets List Test Page" with one widget already placed in
// each of its "Main" and "Sidebar" zones - an empty zone's placeholder
// collapses to zero height and can't reliably receive a simulated drop, so
// the target zone needs a widget of its own to drop onto.
public sealed class WidgetsListDragTests : CmsTestBase<WidgetDragTestsFixture>, IClassFixture<WidgetDragTestsFixture>
{
    public WidgetsListDragTests(WidgetDragTestsFixture fixture) : base(fixture) { }

    private static async Task OpenWidgetsListTestPageAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/WidgetsListTest");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Widgets List Test Page" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task WidgetsListDrag_MoveWidgetToDifferentZone_PersistsOnSave()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenWidgetsListTestPageAsync(page);

        var mainZone = page.Locator(".widget-editor-body[data-zone='Main'] .widget-template-placeholder");
        var sidebarZone = page.Locator(".widget-editor-body[data-zone='Sidebar'] .widget-template-placeholder");

        var firstWidget = mainZone.Locator(".widget-template").First;
        var secondWidget = sidebarZone.Locator(".widget-template").First;
        await firstWidget.WaitForAsync();
        await secondWidget.WaitForAsync();

        await page.DragAsync(firstWidget.Locator(".widget-editor-handle"), secondWidget);

        await Assertions.Expect(sidebarZone.Locator(".widget-template")).ToHaveCountAsync(2);
        await Assertions.Expect(mainZone.Locator(".widget-template")).ToHaveCountAsync(0);

        // Both widgets' hidden "source-zone" fields should already reflect
        // "Sidebar" before saving - the pre-existing widget's own field was
        // already "Sidebar", and the moved widget's should now match it too.
        foreach (var sourceZoneInput in await sidebarZone.Locator("input.source-zone").AllAsync())
        {
            Assert.Equal("Sidebar", await sourceZoneInput.InputValueAsync());
        }

        await page.Locator(".btn.draft").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Saving an existing item (unlike creating a new one) redirects to
        // its returnUrl (the content list), not back to the same edit page -
        // reopen it explicitly to verify persistence.
        await OpenWidgetsListTestPageAsync(page);

        var reloadedSidebar = page.Locator(".widget-editor-body[data-zone='Sidebar'] .widget-template-placeholder");
        var reloadedMain = page.Locator(".widget-editor-body[data-zone='Main'] .widget-template-placeholder");
        await Assertions.Expect(reloadedSidebar.Locator(".widget-template")).ToHaveCountAsync(2);
        await Assertions.Expect(reloadedMain.Locator(".widget-template")).ToHaveCountAsync(0);

        // Unlike MenuHierarchyTests/LayersDragTests/AdminMenuTreeTests, this
        // class has a single test method against a fixture-scoped (fresh per
        // test run) item, so there's no cross-test state to restore. Moving
        // it back would also need a drop target in "Main", which is now
        // genuinely empty (0 elements, not just zero height) and can't
        // receive a simulated drop either.
        await page.CloseAsync();
    }
}
