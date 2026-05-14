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
    public async Task AdminNavigationShouldNotUseAdminQueryParameter()
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
