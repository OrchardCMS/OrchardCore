using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based nested drag-and-drop hierarchy editor on the Menu
// admin edit page ("Main Menu" ships with "Home" and "About" via the Blog
// recipe). See @orchardcore/bloom/components/sortable-menu.ts, also consumed
// by OrchardCore.Taxonomies (see TaxonomyHierarchyTests, which additionally
// covers moving a nested item directly to a different parent in one drag,
// using the Blog recipe's 3-term "Tags" taxonomy).
public sealed class MenuHierarchyTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public MenuHierarchyTests(BlogFixture fixture) : base(fixture) { }

    // Navigates via the admin UI - the Menus list, then the "Main Menu" row's
    // Edit link - rather than a hardcoded content item id, since the Blog
    // recipe generates a fresh id per test run.
    private static async Task OpenMainMenuAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Menu");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Main Menu" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("#menu").WaitForAsync();
    }

    // Saving as a draft redirects back to the Menus list (the edit link
    // includes a returnUrl to it), not back to the same edit page, so
    // persistence has to be verified by navigating to the edit page again.
    private static async Task SaveDraftAsync(IPage page)
    {
        await page.Locator(".btn.draft").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await OpenMainMenuAsync(page);
    }

    [Fact]
    public async Task MenuHierarchy_DragItemUp_ReordersWithoutNestingAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenMainMenuAsync(page);

        await page.DragMenuItemJustBeforeAsync("About", "Home");

        Assert.Equal("0", await page.GetMenuItemDepthAsync("Home"));
        Assert.Equal("0", await page.GetMenuItemDepthAsync("About"));

        var order = await page.Locator("#menu li.menu-item").AllTextContentsAsync();
        Assert.Contains("About", order[0]);
        Assert.Contains("Home", order[1]);

        await SaveDraftAsync(page);

        var reloadedOrder = await page.Locator("#menu li.menu-item").AllTextContentsAsync();
        Assert.Contains("About", reloadedOrder[0]);
        Assert.Contains("Home", reloadedOrder[1]);

        await page.CloseAsync();
    }

    [Fact]
    public async Task MenuHierarchy_DragRight_NestsItemUnderPrecedingSiblingAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenMainMenuAsync(page);

        await page.DragMenuItemSidewaysAsync("About", 70);

        Assert.Equal("1", await page.GetMenuItemDepthAsync("About"));

        await SaveDraftAsync(page);

        Assert.Equal("1", await page.GetMenuItemDepthAsync("About"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task MenuHierarchy_NestThenDragLeft_OutdentsBackToRootRightAfterParentAndPersists()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await OpenMainMenuAsync(page);

        await page.DragMenuItemSidewaysAsync("About", 70); // nest under Home
        await page.DragMenuItemSidewaysAsync("About", -70); // outdent back out

        Assert.Equal("0", await page.GetMenuItemDepthAsync("About"));

        var order = await page.Locator("#menu li.menu-item").AllTextContentsAsync();
        Assert.Contains("Home", order[0]);
        Assert.Contains("About", order[1]);

        await SaveDraftAsync(page);

        Assert.Equal("0", await page.GetMenuItemDepthAsync("About"));

        await page.CloseAsync();
    }
}
