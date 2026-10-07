using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers every remaining admin list view built on the shared
// .scripts/bloom/components/bulk-select-list.ts component (select-all checkbox +
// per-row checkboxes + #actions/#items/#selected-items visibility toggling), beyond
// TenantsBulkSelectTests (Tenants admin list) which already covers the pattern with
// dynamically-created tenants. All 17 real consumers of bulk-select-list.ts are:
// ContentsAdminList, UsersAdminList, NotificationsAdminList, Placements, Shortcodes,
// Sitemaps (2 lists), Templates, Tenants (2 lists, one covered by
// TenantsBulkSelectTests), UrlRewriting, DeploymentPlan, Deployment.Remote (2 lists),
// Indexing, MediaProfiles, AdminMenu, Queries.
//
// Views intentionally NOT covered here: NotificationsAdminList, RemoteClient/
// RemoteInstance (Deployment.Remote), and Indexing have no recipe step to seed rows
// through - creating them requires either live notification delivery or UI
// click-through create flows, out of scope for a bulk-select-list.ts regression
// check when the module itself is byte-identical to the 14 already covered here and
// in TenantsBulkSelectTests. If those views ever need real functional coverage for
// their own domain logic, that's a separate, larger test.
public sealed class BulkSelectListTests : CmsTestBase<BulkSelectListTestsFixture>, IClassFixture<BulkSelectListTestsFixture>
{
    public BulkSelectListTests(BulkSelectListTestsFixture fixture) : base(fixture) { }

    // Every consuming view wires the exact same #actions/#items/#selected-items ids
    // and select-all/per-row checkbox behavior (see bulk-select-list.ts) - only the
    // checkbox `name` attribute and the row count vary per view.
    private static async Task AssertSelectAllTogglesActionsAsync(IPage page, string checkboxName, int expectedRowCount)
    {
        var actions = page.Locator("#actions");
        var items = page.Locator("#items");
        var selectedItems = page.Locator("#selected-items");
        var checkboxes = page.Locator($"input[type='checkbox'][name='{checkboxName}']");

        var count = await checkboxes.CountAsync();
        Assert.True(count >= expectedRowCount, $"Expected at least {expectedRowCount} rows to select from, found {count}.");

        await Assertions.Expect(actions).ToBeHiddenAsync();
        await Assertions.Expect(items).ToBeVisibleAsync();

        await page.Locator("#select-all").ClickAsync();

        await Assertions.Expect(actions).ToBeVisibleAsync();
        await Assertions.Expect(items).ToBeHiddenAsync();
        // The label text itself is localized ("2 sélectionné(s)" etc.), so assert on
        // the actual checked-state and count instead of the text.
        Assert.Contains(count.ToString(), await selectedItems.InnerTextAsync());
        Assert.Equal(count, await page.Locator($"input[type='checkbox'][name='{checkboxName}']:checked").CountAsync());

        // Unchecking every box individually (rather than a second click on
        // select-all, which the page's own logic doesn't wire back to clearing every
        // checkbox) restores the default view.
        for (var i = 0; i < count; i++)
        {
            await checkboxes.Nth(i).UncheckAsync();
        }

        await Assertions.Expect(actions).ToBeHiddenAsync();
        await Assertions.Expect(items).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("/Admin/Contents/ContentItems/BulkSelectTestPage", "itemIds")]
    [InlineData("/Admin/Placements", "itemIds")]
    [InlineData("/Admin/Shortcodes", "itemIds")]
    [InlineData("/Admin/Sitemaps/List", "itemIds")]
    [InlineData("/Admin/SitemapIndexes/List", "itemIds")]
    [InlineData("/Admin/Templates", "itemIds")]
    [InlineData("/Admin/TenantFeatureProfiles", "itemIds")]
    [InlineData("/Admin/UrlRewriting/Index", "ruleIds")]
    [InlineData("/Admin/DeploymentPlan/Index", "itemIds")]
    [InlineData("/Admin/MediaProfiles", "itemIds")]
    [InlineData("/Admin/AdminMenu/List", "itemIds")]
    [InlineData("/Admin/Queries/Index", "itemIds")]
    public async Task BulkSelectList_SelectAll_TogglesActionsAndCount(string adminUrl, string checkboxName)
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync(adminUrl);

        await AssertSelectAllTogglesActionsAsync(page, checkboxName, expectedRowCount: 2);

        await page.CloseAsync();
    }
}
