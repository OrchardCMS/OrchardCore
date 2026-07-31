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
