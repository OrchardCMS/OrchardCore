using System.Text.RegularExpressions;
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
    //
    // SKIPPED-then-fixed: intermittently flaky in CI across every DB backend (Redis+Azurite,
    // Postgres, MySql), independent of runner speed. The first fix attempt (an auto-retrying
    // wait BETWEEN the two drags) reduced but did not eliminate the failure rate, because the
    // final order/depth checks AFTER the second drag were still plain, non-retrying reads
    // (Assert.Equal / AllTextContentsAsync) - exactly the same race, just one step later:
    // under a CPU-starved runner, SortableJS's onEnd handler (and the resulting re-render) can
    // still be mid-flight when those reads fire. Replaced with Playwright's own auto-retrying
    // locator assertions (ToHaveAttributeAsync / ToHaveTextAsync), which wait for the DOM to
    // actually reach the expected state instead of assuming it already has by the time the
    // read happens.
    [Fact]
    public async Task TaxonomyHierarchy_MoveNestedTermNearDifferentSibling_ReparentsInOneDragAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenTagsAsync(page);

        var menuItems = page.Locator("#menu li.menu-item");

        await page.DragMenuItemSidewaysAsync("Exploration", 70); // nest under Earth

        // Wait for the nest to actually settle before starting the next drag:
        // a fixed sleep inside DragMenuItemSidewaysAsync isn't always enough
        // headroom on a slower CI runner, and starting the second drag before
        // the first one's DOM update has fully applied races the two together.
        await Assertions.Expect(menuItems.Filter(new LocatorFilterOptions { HasText = "Exploration" }).First)
            .ToHaveAttributeAsync("data-depth", "1");

        await page.DragMenuItemJustAfterAsync("Exploration", "Space");

        // Same race, one step later: wait for the second drag's DOM update to actually
        // settle (auto-retrying) rather than reading depth/order once and assuming it's
        // already final.
        await Assertions.Expect(menuItems.Filter(new LocatorFilterOptions { HasText = "Exploration" }).First)
            .ToHaveAttributeAsync("data-depth", "1");
        await Assertions.Expect(menuItems).ToHaveTextAsync(
            [new Regex("Earth"), new Regex("Space"), new Regex("Exploration")]);

        await SaveDraftAsync(page);

        var reloadedMenuItems = page.Locator("#menu li.menu-item");
        await Assertions.Expect(reloadedMenuItems.Filter(new LocatorFilterOptions { HasText = "Exploration" }).First)
            .ToHaveAttributeAsync("data-depth", "1");
        await Assertions.Expect(reloadedMenuItems).ToHaveTextAsync(
            [new Regex("Earth"), new Regex("Space"), new Regex("Exploration")]);

        await page.CloseAsync();
    }
}
