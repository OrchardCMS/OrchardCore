using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class BlogTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    // The cookie may be URL-encoded more than once depending on the browser API path.
    private const int MaxCookieDecodeAttempts = 3;

    public BlogTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheHomePageOfTheBlogRecipe()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");
        await Assertions.Expect(page.Locator(".subheading")).ToContainTextAsync("This is the description of your blog");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminNavigationShouldNotUseAdminQueryParameter()
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

        var prefs = ParseCookieJson(prefsCookie.Value);
        var selectedNavHash = prefs?["selectedNavHash"]?.GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(selectedNavHash));

        await page.CloseAsync();
    }

    private static JsonNode ParseCookieJson(string value)
    {
        var raw = value;

        for (var i = 0; i < MaxCookieDecodeAttempts; i++)
        {
            try
            {
                return JsonNode.Parse(raw);
            }
            catch (JsonException) when (raw.Contains('%'))
            {
                raw = Uri.UnescapeDataString(raw);
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
