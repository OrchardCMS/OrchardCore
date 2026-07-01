using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based nested drag-and-drop hierarchy editor on the
// Taxonomy admin edit page ("Tags" ships with "Earth", "Exploration" and
// "Space" via the Blog recipe). See @orchardcore/bloom/components/sortable-menu.ts,
// also consumed by OrchardCore.Menu (see MenuHierarchyTests, which covers
// plain reordering/indent/outdent using the recipe's 2-item "Main Menu").
public sealed class TaxonomyHierarchyTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public TaxonomyHierarchyTests(BlogFixture fixture) : base(fixture) { }

    // Navigates via the admin UI - the Taxonomies list, then the "Tags" row's
    // Edit link - rather than a hardcoded content item id, since the Blog
    // recipe generates a fresh id per test run.
    private static async Task OpenTagsAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Taxonomy");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Tags" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("#menu").WaitForAsync();
    }

    // Saving as a draft redirects back to the Taxonomies list (the edit link
    // includes a returnUrl to it), not back to the same edit page, so
    // persistence has to be verified by navigating to the edit page again.
    private static async Task SaveDraftAsync(IPage page)
    {
        await page.Locator(".btn.draft").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await OpenTagsAsync(page);
    }

    // This is the scenario that motivated moving from the old per-parent nested
    // <ol> model (where an item could only reorder among its current siblings,
    // outdent to its own parent's level, or indent under its current preceding
    // sibling) to the current flat depth-tagged list: with the old model, moving
    // a nested item to a DIFFERENT parent required first dragging it back out to
    // the root level as a separate step, then a second drag to indent it under
    // the new parent. Here, "Exploration" moves directly from being nested under
    // "Earth" to being nested under "Space" in a single drag, because its depth
    // is clamped to whatever's still valid at its new position (dropped right
    // after another root item, it re-nests under that item automatically) - no
    // explicit sideways gesture, and no intermediate outdent step, needed.
    [Fact]
    public async Task TaxonomyHierarchy_MoveNestedTermNearDifferentSibling_ReparentsInOneDragAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenTagsAsync(page);

        await page.DragMenuItemSidewaysAsync("Exploration", 70); // nest under Earth
        Assert.Equal("1", await page.GetMenuItemDepthAsync("Exploration"));

        await page.DragMenuItemJustAfterAsync("Exploration", "Space");

        Assert.Equal("1", await page.GetMenuItemDepthAsync("Exploration"));

        var order = await page.Locator("#menu li.menu-item").AllTextContentsAsync();
        Assert.Contains("Earth", order[0]);
        Assert.Contains("Space", order[1]);
        Assert.Contains("Exploration", order[2]);

        await SaveDraftAsync(page);

        Assert.Equal("1", await page.GetMenuItemDepthAsync("Exploration"));

        var reloadedOrder = await page.Locator("#menu li.menu-item").AllTextContentsAsync();
        Assert.Contains("Earth", reloadedOrder[0]);
        Assert.Contains("Space", reloadedOrder[1]);
        Assert.Contains("Exploration", reloadedOrder[2]);

        await page.CloseAsync();
    }
}
