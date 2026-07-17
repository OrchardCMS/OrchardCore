using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminNavigationTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    // The cookie may be URL-encoded more than once depending on the browser API path.
    private const int MaxCookieDecodeAttempts = 3;

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

        var prefsCookie = (await page.Context.CookiesAsync())
            .FirstOrDefault(c => c.Name.EndsWith("-adminPreferences", StringComparison.Ordinal));
        Assert.NotNull(prefsCookie);

        var prefs = ParseAndDecodeCookieJson(prefsCookie.Value);
        var selectedNavHash = prefs?["selectedNavHash"]?.GetValue<string>();
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

    [Fact]
    public async Task AdminNavigation_DirectNavigation_NotKeepPreviousMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        await page.GetByRole(AriaRole.Button, new() { Name = "Content", Exact = true }).ClickAsync();

        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/Admin/Features\"]").First;
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        // Clicking the nav link stores its hash in the admin preferences cookie.
        await articleLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);

        // Direct navigation (bookmark, typed URL, in-page link) to a page whose URL
        // exactly matches another menu item must win over the stored hash.
        await page.GotoAndAssertOkAsync("/Admin/Features");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_ContentItemEditBookmark_ActivatesContentTypeMenuItem()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Find the edit URL of the "About" article from its content type list page.
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Article");
        var aboutEditUrl = await page.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true }).GetAttributeAsync("href");
        Assert.Contains("/Edit", aboutEditUrl, StringComparison.OrdinalIgnoreCase);

        // Simulate a bookmark: no admin preferences cookie, so no clicked-hash fallback exists.
        var prefsCookie = (await page.Context.CookiesAsync())
            .FirstOrDefault(c => c.Name.EndsWith("-adminPreferences", StringComparison.Ordinal));
        if (prefsCookie != null)
        {
            await page.Context.ClearCookiesAsync(new() { Name = prefsCookie.Name });
        }

        await page.GotoAndAssertOkAsync(aboutEditUrl);

        // The edit page declares the Article list page as its owner, so the Article menu item
        // must be active even without any URL match or stored preference.
        var articleLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var contentItemsLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems\"]");
        var activeArticleItem = articleLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var activeContentItemsItem = contentItemsLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeArticleItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeContentItemsItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AdminNavigation_LinkToContentItemEdit_KeepsLinkMenuItemActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        // "Main Menu" is an admin menu link pointing directly at the edit page of a Menu content
        // item, so it must stay active instead of the list page of the Menu content type, which
        // that edit page declares as its owner.
        var mainMenuLink = page.GetByRole(AriaRole.Link, new() { Name = "Main Menu", Exact = true });
        await mainMenuLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Edit", page.Url, StringComparison.OrdinalIgnoreCase);

        var activeMainMenuItem = mainMenuLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");
        var menusLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Menu\"]");
        var activeMenusItem = menusLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeMainMenuItem).ToHaveCountAsync(1);
        await Assertions.Expect(activeMenusItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    private static JsonNode ParseAndDecodeCookieJson(string value)
    {
        var raw = value;

        for (var i = 0; i < MaxCookieDecodeAttempts; i++)
        {
            try
            {
                return JsonNode.Parse(raw);
            }
            catch (JsonException)
            {
                var decoded = Uri.UnescapeDataString(raw);
                if (decoded == raw)
                {
                    break;
                }

                raw = decoded;
            }
        }

        try
        {
            return JsonNode.Parse(raw);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Unable to parse the admin preferences cookie value as JSON.", exception);
        }
    }
}
