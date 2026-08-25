using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Sitemaps' SitemapsCache admin page (/Admin/SitemapsCache/List):
// the client-side search box and the per-item Purge / global Purge All actions.
//
// NOTE: sitemap-cache-list.ts (the plan doc's originally-named target file) was deleted
// as part of this task - it was fully dead code (queried [data-cacheentry]/#cache-entries,
// neither of which the view has rendered since an earlier, unrelated "Reusable List
// Management Script" migration switched this page over to data-filter-value + the shared
// list-management.js). Confirmed with the user before deleting; see the commit message for
// the full explanation.
//
// REAL BUG FOUND AND FIXED as part of this task: SitemapCache/List.cshtml carried the
// data-list-management/data-client-side-search="true" wrapper attributes but was MISSING
// the <script asp-name="list-management" at="Foot"></script> include entirely, so
// window.listManagement never loaded and the search box's listeners were never wired up -
// the search silently did nothing, confirmed via page.EvaluateAsync checking
// `typeof window.listManagement`. Fixed by adding the missing script tag (see every other
// data-list-management consumer, e.g. OrchardCore.ContentTypes/Views/Admin/List.cshtml, for
// the same pattern). This test's search assertions are the regression coverage for that fix.
public sealed class SitemapCacheTests : CmsTestBase<SitemapCacheTestsFixture>, IClassFixture<SitemapCacheTestsFixture>
{
    public SitemapCacheTests(SitemapCacheTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task SitemapCache_SearchFiltersEntries_PurgeAndPurgeAllRemoveEntries()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Visiting the public sitemap XML endpoint populates the file cache (see
        // SitemapController.Index -> ISitemapCacheProvider.SetSitemapCacheAsync).
        await page.GotoAndAssertOkAsync("/sitemap-cache-test-sitemap");
        Assert.Contains("xml", (await page.ContentAsync()).ToLowerInvariant());

        await page.GotoAndAssertOkAsync("/Admin/SitemapsCache/List");

        var cacheItems = page.Locator("form ul.list-group li.list-group-item");
        var initialCount = await cacheItems.CountAsync();
        Assert.True(initialCount >= 1, "Expected at least one cached sitemap entry after visiting the public sitemap URL.");
        var cachedFileName = (await cacheItems.First.GetAttributeAsync("data-filter-value"))!;
        Assert.False(string.IsNullOrEmpty(cachedFileName));
        var targetItem = page.Locator($"form ul.list-group li.list-group-item[data-filter-value='{cachedFileName}']").First;

        // The shared list-management.js drives this page's client-side search (see
        // data-list-management/data-client-side-search="true" on List.cshtml's wrapper).
        // Search by this run's actual cached filename, so this passes even when prior runs
        // left other cache entries in place.
        var searchBox = page.Locator("#search-box");
        await searchBox.FillAsync(cachedFileName);
        await Assertions.Expect(targetItem).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bd-none\\b"));
        await Assertions.Expect(page.Locator("#list-alert")).ToBeHiddenAsync();

        await searchBox.FillAsync("this-will-not-match-anything");
        await Assertions.Expect(targetItem).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bd-none\\b"));
        await Assertions.Expect(page.Locator("#list-alert")).ToBeVisibleAsync();

        await searchBox.FillAsync("");
        await Assertions.Expect(targetItem).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bd-none\\b"));

        // Purging this specific cached entry removes only it. data-url-af links go through
        // TheAdmin's confirmDialog() modal (see TheAdmin.ts) rather than a native browser
        // confirm() dialog - #modalOkButton is the dialog's confirm action.
        await targetItem.Locator("a").Filter(new LocatorFilterOptions { HasText = "Purge" }).ClickAsync();
        await Assertions.Expect(page.Locator("#confirmRemoveModal")).ToBeVisibleAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(page.Locator($"form ul.list-group li.list-group-item[data-filter-value='{cachedFileName}']")).ToHaveCountAsync(0);

        // Purge All clears everything, regardless of how many entries remain.
        await page.Locator("a").Filter(new LocatorFilterOptions { HasText = "Purge All" }).ClickAsync();
        await Assertions.Expect(page.Locator("#confirmRemoveModal")).ToBeVisibleAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(page.Locator("form ul.list-group")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#list-empty")).ToBeVisibleAsync();

        // Repopulate the cache, then confirm Purge All also clears a freshly-populated cache.
        await page.GotoAndAssertOkAsync("/sitemap-cache-test-sitemap");
        await page.GotoAndAssertOkAsync("/Admin/SitemapsCache/List");
        Assert.True(await page.Locator("form ul.list-group li.list-group-item").CountAsync() >= 1);

        await page.Locator("a").Filter(new LocatorFilterOptions { HasText = "Purge All" }).ClickAsync();
        await Assertions.Expect(page.Locator("#confirmRemoveModal")).ToBeVisibleAsync();
        await page.Locator("#modalOkButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(page.Locator("form ul.list-group")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#list-empty")).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
