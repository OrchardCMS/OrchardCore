using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminNavigationTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public AdminNavigationTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AdminNavigation_Default_NotUseAdminQueryParameter()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        // Admin links persisted by TheAdmin include data-admin-hash and local admin hrefs.
        var adminLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/\"]").First;
        await adminLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("?admin=", page.Url, StringComparison.OrdinalIgnoreCase);

        var selectedNavHash = await page.EvaluateAsync<string>("""
            () => {
                const tenant = document.documentElement.getAttribute('data-tenant') ?? '';
                return sessionStorage.getItem(`${tenant}-selectedNavHash`);
            }
            """);

        Assert.False(string.IsNullOrWhiteSpace(selectedNavHash));

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminRoot_Default_NotKeepPreviousMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Features");

        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/Admin/Features\"]").First;
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);

        await page.GotoAndAssertOkAsync("/Admin");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_ContentTypeEdit_KeepsContentTypeMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();

        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var contentItemsLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems\"]");
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeContentItemsItem = contentItemsLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await articleLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Edit", page.Url, StringComparison.OrdinalIgnoreCase);
        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

}
