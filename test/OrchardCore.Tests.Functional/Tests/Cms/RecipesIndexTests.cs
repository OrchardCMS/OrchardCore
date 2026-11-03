using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Recipes' hand-rolled admin search/filter (recipes-index.ts) - unlike
// most other admin list pages, this one does NOT use the shared list-management search
// components (data-list-management), it wires its own keyup listener directly.
public sealed class RecipesIndexTests : CmsTestBase<RecipesIndexTestsFixture>, IClassFixture<RecipesIndexTestsFixture>
{
    public RecipesIndexTests(RecipesIndexTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SearchBox_FiltersToMatchingRecipesOnly()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Recipes");

        // OrchardCore.Media ships 3 named recipes; search for "Media API" (unique to
        // that module's own recipe display names - plain "PKCE" also matches an
        // OpenApi-module recipe pulled in as a dependency).
        var allItems = page.Locator(".recipe-group ul.list-group li.list-group-item");
        var initialCount = await allItems.CountAsync();
        Assert.True(initialCount >= 3, $"Expected at least 3 recipe rows, found {initialCount}.");

        var searchBox = page.Locator("#search-box");
        await searchBox.FillAsync("Media API");
        // recipes-index.ts listens on "keyup", not "input" - Playwright's FillAsync alone
        // may not dispatch it, so press a key to make sure the real handler fires.
        await searchBox.PressAsync("End");

        var visibleItems = allItems.Locator("visible=true");
        await Assertions.Expect(visibleItems).ToHaveCountAsync(3);
        var visibleTexts = await visibleItems.AllTextContentsAsync();
        Assert.All(visibleTexts, text => Assert.Contains("Media API", text));

        // Clearing via Escape (the script's own documented shortcut) should restore all rows.
        await searchBox.PressAsync("Escape");
        await Assertions.Expect(allItems.Locator("visible=true")).ToHaveCountAsync(initialCount);

        // A search matching nothing should show the "no results" alert.
        await searchBox.FillAsync("ZzzNoSuchRecipeZzz");
        await searchBox.PressAsync("End");
        await Assertions.Expect(allItems.Locator("visible=true")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#list-alert")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bd-none\b"));

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
