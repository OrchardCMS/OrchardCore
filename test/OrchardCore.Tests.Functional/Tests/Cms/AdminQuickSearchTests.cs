using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminQuickSearchTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public AdminQuickSearchTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task QuickSearch_CtrlK_OpensPaletteAndNavigates()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickSearchModal");
        var input = page.Locator("#adminQuickSearchInput");

        await Assertions.Expect(modal).ToBeVisibleAsync();
        await Assertions.Expect(input).ToBeFocusedAsync();

        await input.FillAsync("features");

        var firstOption = modal.Locator("[role=option]").First;
        await Assertions.Expect(firstOption.Locator(".admin-quick-search-title")).ToHaveTextAsync("Features");
        await Assertions.Expect(firstOption.Locator(".admin-quick-search-path")).ToContainTextAsync("Tools");

        await Task.WhenAll(
            page.WaitForURLAsync("**/Admin/Features**"),
            page.Keyboard.PressAsync("Enter"));

        Assert.Contains("/Admin/Features", page.Url, StringComparison.OrdinalIgnoreCase);

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickSearch_NavbarButton_OpensAndEscapeCloses()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        var modal = page.Locator("#adminQuickSearchModal");

        await page.Locator("#adminQuickSearchToggle").ClickAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#adminQuickSearchInput")).ToBeFocusedAsync();

        // With an empty term the palette lists the first menu items.
        await Assertions.Expect(modal.Locator("[role=option]").First).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(modal).ToBeHiddenAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickSearch_DiacriticTerm_MatchesTitle()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickSearchModal");
        await Assertions.Expect(modal).ToBeVisibleAsync();

        await page.Locator("#adminQuickSearchInput").FillAsync("fêatures");

        await Assertions.Expect(modal.Locator("[role=option]").First.Locator(".admin-quick-search-title")).ToHaveTextAsync("Features");

        await page.CloseAsync();
    }

    [Fact]
    public async Task QuickSearch_NoMatch_ShowsNoResults()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.Keyboard.PressAsync("Control+k");

        var modal = page.Locator("#adminQuickSearchModal");
        await Assertions.Expect(modal).ToBeVisibleAsync();

        await page.Locator("#adminQuickSearchInput").FillAsync("zzzz-no-such-menu-item");

        await Assertions.Expect(modal.Locator("[role=option]")).ToHaveCountAsync(0);
        await Assertions.Expect(modal.Locator(".admin-quick-search-empty")).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
