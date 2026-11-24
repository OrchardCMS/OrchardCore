using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the editor of a menu item, which saves the item in its menu instead of publishing it, and warns when the
// menu has unpublished changes, and the rows of the admin menu node tree, which follow the "Actions layout" admin
// setting. The site is set up with the Blog recipe ("Main Menu" ships published with "Home" and "About", and the
// "Admin menus" tree with five nodes) and the 'Menu' actions layout.
public sealed class MenuItemEditorTests : CmsTestBase<MenuItemEditorTestsFixture>, IClassFixture<MenuItemEditorTestsFixture>
{
    private const string DraftWarning = "Visitors won't see your changes until the menu is published.";

    public MenuItemEditorTests(MenuItemEditorTestsFixture fixture) : base(fixture) { }

    // The row actions of the admin lists are in a closed dropdown in the 'Menu' actions layout, so the pages are
    // opened from the links of the rows rather than by clicking them.
    private static async Task GotoLinkAsync(IPage page, ILocator link)
    {
        await page.GotoAndAssertOkAsync(await link.GetAttributeAsync("href"));
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task OpenMainMenuAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Menu");
        await GotoLinkAsync(page, page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Main Menu" })
            .Locator("a.edit").First);
        await page.Locator("#menu").WaitForAsync();
    }

    // The actions of the menu items are in the 'Menu' actions layout, so the Edit link is opened from the dropdown.
    private static async Task OpenMenuItemAsync(IPage page, string name)
    {
        var row = page.Locator("#menu li.menu-item").Filter(new LocatorFilterOptions { HasText = name }).First;
        await row.Locator(".admin-list-actions-menu > button").ClickAsync();
        await row.Locator(".dropdown-menu a").Filter(new LocatorFilterOptions { HasText = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Fact]
    public async Task MenuItemEditor_SaveMenuItem_SavesInADraftOfTheMenuAndWarnsAboutIt()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await OpenMainMenuAsync(page);
        await OpenMenuItemAsync(page, "Home");

        // The menu is published: the menu item is saved, not published, and there is nothing to warn about.
        var actionBar = page.Locator(".action-bar-bottom");
        await Assertions.Expect(actionBar.GetByRole(AriaRole.Button, new() { Name = "Save Menu Item" })).ToBeVisibleAsync();
        await Assertions.Expect(actionBar.Locator("button.publish, button.draft")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByText(DraftWarning)).ToHaveCountAsync(0);

        await actionBar.GetByRole(AriaRole.Button, new() { Name = "Save Menu Item" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("#menu").WaitForAsync();

        // Saving the menu item saved a draft of the menu, which its items now warn about until it is published.
        await OpenMenuItemAsync(page, "Home");
        await Assertions.Expect(page.GetByText(DraftWarning)).ToBeVisibleAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminMenuNodeList_MenuActionsLayout_RendersTheActionsOfEachNodeInAMenu()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/AdminMenu/List");
        await GotoLinkAsync(page, page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Admin menus" })
            .Locator("a").Filter(new LocatorFilterOptions { HasText = "Edit Nodes" }).First);
        await page.Locator("#menu").WaitForAsync();

        var nodes = page.Locator("#menu li.menu-item");
        var nodeCount = await nodes.CountAsync();
        Assert.True(nodeCount > 0);
        await Assertions.Expect(page.Locator("#menu .admin-list-actions-menu")).ToHaveCountAsync(nodeCount);
        await Assertions.Expect(page.Locator("#menu .admin-list-actions-buttons")).ToHaveCountAsync(0);

        var actions = nodes.First.Locator(".admin-list-actions-menu").First;
        await actions.Locator("> button").ClickAsync();
        Assert.Equal(["Edit", "Delete", "Disable"], (await actions.Locator(".dropdown-menu a").AllTextContentsAsync())
            .Select(text => text.Trim()).ToArray());

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}

public sealed class MenuItemEditorTestsFixture : CmsRecipeFixture
{
    public MenuItemEditorTestsFixture()
        : base(builder => builder.Configuration["OrchardCore:Admin:List:DefaultActionsLayout"] = "Menu")
    {
    }

    protected override string RecipeName => "Blog";
}
