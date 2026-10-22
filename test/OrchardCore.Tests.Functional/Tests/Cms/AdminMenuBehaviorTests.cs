using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the 'Navigation menu behavior' admin setting. The tests of this class share one site, so
// each of them sets the behavior it needs first.
public sealed class AdminMenuBehaviorTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    private const string ArticleHref = "/Admin/Contents/ContentItems/Article";
    private const string PlacementsHref = "/Admin/Placements";

    public AdminMenuBehaviorTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task NavigateAway_PersistentMenu_KeepsOpenedSectionsOpen()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await SetMenuBehaviorAsync(page, "Persistent");

        await page.GotoAndAssertOkAsync("/Admin");
        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true }).ClickAsync();

        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeVisibleAsync();
        await Assertions.Expect(MenuLink(page, PlacementsHref)).ToBeVisibleAsync();

        // Neither section holds the page navigated to, they stay open because the user opened them.
        await page.GotoAndAssertOkAsync("/Admin/Features");

        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeVisibleAsync();
        await Assertions.Expect(MenuLink(page, PlacementsHref)).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task OpenSection_FocusedMenu_ClosesTheOtherSections()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await SetMenuBehaviorAsync(page, "Focused");

        await page.GotoAndAssertOkAsync("/Admin");
        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();
        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true }).ClickAsync();

        await Assertions.Expect(MenuLink(page, PlacementsHref)).ToBeVisibleAsync();
        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeHiddenAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task NavigateAway_FocusedMenu_OpensOnlyTheCurrentSection()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await SetMenuBehaviorAsync(page, "Focused");

        await page.GotoAndAssertOkAsync("/Admin");
        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();
        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeVisibleAsync();

        // Placements is in the Design section: that section opens, the one opened before does not stay open.
        await page.GotoAndAssertOkAsync(PlacementsHref);

        await Assertions.Expect(MenuLink(page, PlacementsHref)).ToBeVisibleAsync();
        await Assertions.Expect(MenuLink(page, ArticleHref)).ToBeHiddenAsync();

        await page.CloseAsync();
    }

    private static ILocator MenuLink(IPage page, string href)
        => page.Locator($"#adminMenu a[href=\"{href}\"]");

    private static async Task SetMenuBehaviorAsync(IPage page, string behavior)
    {
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await page.Locator($"input[type='radio'][id$='MenuBehavior_{behavior}']").CheckAsync();
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The menu renders the behavior it applies: waiting on it proves the setting is in effect.
        await page.GotoAndAssertOkAsync("/Admin");
        await Assertions.Expect(page.Locator("#left-nav")).ToHaveAttributeAsync("data-menu-behavior", behavior.ToLowerInvariant());
    }
}
